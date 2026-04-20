import fs from "node:fs";
import path from "node:path";
import process from "node:process";
import { chromium } from "playwright";

function log(message) {
  console.log(`[web-video] ${message}`);
}

function readArgs(argv) {
  const args = {};
  for (let index = 0; index < argv.length; index += 1) {
    const item = argv[index];
    if (!item.startsWith("--")) {
      continue;
    }

    const key = item.slice(2);
    const next = argv[index + 1];
    if (!next || next.startsWith("--")) {
      args[key] = true;
      continue;
    }

    args[key] = next;
    index += 1;
  }

  return args;
}

function ensureDirectory(directoryPath) {
  fs.mkdirSync(directoryPath, { recursive: true });
}

function sanitizeFilePart(value) {
  return String(value ?? "item")
    .replace(/[^a-zA-Z0-9-_]+/g, "-")
    .replace(/-+/g, "-")
    .replace(/^-|-$/g, "")
    .toLowerCase();
}

function loadManifest(manifestPath) {
  const content = fs.readFileSync(manifestPath, "utf8");
  const manifest = JSON.parse(content);
  const tokenMap = {
    amigos: process.env.VIDEO_SLUG_AMIGOS || "amigos-da-pesca-2026",
    rei: process.env.VIDEO_SLUG_REI || "rei-dos-mares-2026",
    bts: process.env.VIDEO_SLUG_BTS || "bts-sport-fishing-2026",
    torneioid_amigos: process.env.VIDEO_TORNEIOID_AMIGOS || "",
    torneioid_rei: process.env.VIDEO_TORNEIOID_REI || "",
    torneioid_bts: process.env.VIDEO_TORNEIOID_BTS || ""
  };

  if (manifest.authProfiles) {
    for (const [profileName, profile] of Object.entries(manifest.authProfiles)) {
      const envPrefix = `VIDEO_WEB_${profileName.toUpperCase().replace(/[^A-Z0-9]/g, "_")}`;
      profile.username = process.env[`${envPrefix}_USERNAME`] || profile.username;
      profile.password = process.env[`${envPrefix}_PASSWORD`] || profile.password;
      profile.loginPath = replaceManifestTokens(profile.loginPath, tokenMap);
      profile.successUrlPath = replaceManifestTokens(profile.successUrlPath, tokenMap);
    }
  }

  if (process.env.VIDEO_WEB_BASE_URL) {
    manifest.baseUrl = process.env.VIDEO_WEB_BASE_URL;
  }

  if (Array.isArray(manifest.scenes)) {
    for (const scene of manifest.scenes) {
      if (scene.path) {
        scene.path = replaceManifestTokens(scene.path, tokenMap);
      }
      if (scene.url) {
        scene.url = replaceManifestTokens(scene.url, tokenMap);
      }
      if (scene.waitForSelector) {
        scene.waitForSelector = replaceManifestTokens(scene.waitForSelector, tokenMap);
      }
      if (scene.clickSelector) {
        scene.clickSelector = replaceManifestTokens(scene.clickSelector, tokenMap);
      }
      if (Array.isArray(scene.steps)) {
        for (const step of scene.steps) {
          if (step.selector) {
            step.selector = replaceManifestTokens(step.selector, tokenMap);
          }
          if (step.value) {
            step.value = replaceManifestTokens(step.value, tokenMap);
          }
          if (step.label) {
            step.label = replaceManifestTokens(step.label, tokenMap);
          }
          if (step.pattern) {
            step.pattern = replaceManifestTokens(step.pattern, tokenMap);
          }
          if (step.name) {
            step.name = replaceManifestTokens(step.name, tokenMap);
          }
        }
      }
    }
  }

  return manifest;
}

function replaceManifestTokens(value, tokenMap) {
  if (!value || typeof value !== "string") {
    return value;
  }

  return value
    .replaceAll("{slug_amigos}", tokenMap.amigos)
    .replaceAll("{slug_rei}", tokenMap.rei)
    .replaceAll("{slug_bts}", tokenMap.bts)
    .replaceAll("{torneioid_amigos}", tokenMap.torneioid_amigos)
    .replaceAll("{torneioid_rei}", tokenMap.torneioid_rei)
    .replaceAll("{torneioid_bts}", tokenMap.torneioid_bts);
}

function replaceRuntimeTokens(value, runId) {
  if (!value || typeof value !== "string") {
    return value;
  }

  return value.replaceAll("{run_id}", runId);
}

function resolveSceneUrl(baseUrl, scene) {
  if (scene.url) {
    return new URL(scene.url, baseUrl).toString();
  }

  if (scene.path) {
    return new URL(scene.path, baseUrl).toString();
  }

  return baseUrl;
}

async function performLogin(page, manifest, authProfileName) {
  const auth = manifest.authProfiles?.[authProfileName];
  if (!auth) {
    throw new Error(`Perfil de autenticacao web nao encontrado: ${authProfileName}`);
  }

  await page.context().clearCookies();
  const loginUrl = new URL(auth.loginPath ?? "/", manifest.baseUrl).toString();
  log(`Login com perfil ${authProfileName} em ${loginUrl}`);
  await page.goto(loginUrl, { waitUntil: "networkidle" });

  if (auth.usernameSelector && auth.username) {
    log(`Preenchendo usuario em ${auth.usernameSelector}`);
    await page.fill(auth.usernameSelector, auth.username);
  }

  if (auth.passwordSelector && auth.password) {
    log(`Preenchendo senha em ${auth.passwordSelector}`);
    await page.fill(auth.passwordSelector, auth.password);
  }

  if (auth.submitSelector) {
    log(`Submetendo login via ${auth.submitSelector}`);
    await Promise.all([
      page.waitForLoadState("networkidle", { timeout: 30000 }),
      page.click(auth.submitSelector)
    ]);
  }

  if (auth.successUrlPath) {
    log(`Aguardando URL de sucesso: **${auth.successUrlPath}`);
    await page.waitForURL(`**${auth.successUrlPath}`, { timeout: 30000 });
  }

  if (auth.successSelector) {
    log(`Aguardando seletor de sucesso: ${auth.successSelector}`);
    await page.waitForSelector(auth.successSelector, { timeout: 30000 });
  }

  log(`Login concluido para ${authProfileName}`);
}

async function waitSeconds(page, seconds) {
  await page.waitForTimeout(seconds * 1000);
}

async function runSceneSteps(page, scene, runId) {
  if (!Array.isArray(scene.steps)) {
    return;
  }

  for (const step of scene.steps) {
    switch (step.type) {
      case "click":
        log(`Cena step: click ${step.selector}`);
        if (step.acceptDialog) {
          page.once("dialog", async (dialog) => {
            await dialog.accept();
          });
        }
        await page.click(replaceRuntimeTokens(step.selector, runId));
        log(`Cena step: click finalizado. URL atual: ${page.url()}`);
        break;
      case "fill":
        log(`Cena step: fill ${step.selector}`);
        await page.fill(replaceRuntimeTokens(step.selector, runId), replaceRuntimeTokens(step.value, runId));
        break;
      case "select":
        log(`Cena step: select ${step.selector}`);
        await page.selectOption(replaceRuntimeTokens(step.selector, runId), { label: replaceRuntimeTokens(step.label, runId) });
        break;
      case "submit":
        log(`Cena step: submit ${step.selector}`);
        if (step.acceptDialog) {
          page.once("dialog", async (dialog) => {
            await dialog.accept();
          });
        }
        await Promise.all([
          page.waitForLoadState("networkidle", { timeout: 30000 }),
          page.click(replaceRuntimeTokens(step.selector, runId))
        ]);
        log(`Cena step: submit finalizado. URL atual: ${page.url()}`);
        break;
      case "ensureHiddenValue":
        log(`Cena step: ensureHiddenValue ${step.name}`);
        await page.evaluate(({ fieldName, fieldValue }) => {
          const form = document.querySelector("form");
          if (!form) {
            throw new Error("Formulario nao encontrado para ensureHiddenValue.");
          }

          let input = form.querySelector(`input[name="${fieldName}"]`);
          if (!input) {
            input = document.createElement("input");
            input.setAttribute("type", "hidden");
            input.setAttribute("name", fieldName);
            form.appendChild(input);
          }

          input.setAttribute("value", fieldValue);
        }, { fieldName: step.name, fieldValue: replaceRuntimeTokens(step.value, runId) });
        break;
      case "waitForSelector":
        log(`Cena step: waitForSelector ${step.selector}`);
        await page.waitForSelector(replaceRuntimeTokens(step.selector, runId), { timeout: 30000 });
        break;
      case "waitForURL":
        log(`Cena step: waitForURL ${step.pattern}`);
        await page.waitForURL(`**${replaceRuntimeTokens(step.pattern, runId)}`, { timeout: 30000 });
        log(`Cena step: waitForURL concluido. URL atual: ${page.url()}`);
        break;
      default:
        throw new Error(`Tipo de step nao suportado: ${step.type}`);
    }
  }
}

async function captureScene(page, scene, baseUrl, outputDir, sceneIndex, runId) {
  const sceneSlug = sanitizeFilePart(`${String(sceneIndex).padStart(2, "0")}-${scene.title}`);
  const targetUrl = resolveSceneUrl(baseUrl, scene);
  log(`Cena ${sceneIndex}: ${scene.title} -> ${targetUrl}`);
  try {
    await page.goto(targetUrl, { waitUntil: "networkidle" });

    if (scene.waitForSelector) {
      log(`Cena ${sceneIndex}: aguardando seletor ${scene.waitForSelector}`);
      await page.waitForSelector(scene.waitForSelector, { timeout: scene.timeoutMs ?? 30000 });
    }

    if (scene.clickSelector) {
      log(`Cena ${sceneIndex}: clicando em ${scene.clickSelector}`);
      await Promise.all([
        page.waitForLoadState("networkidle", { timeout: scene.timeoutMs ?? 30000 }),
        page.click(scene.clickSelector)
      ]);
      log(`Cena ${sceneIndex}: clickSelector finalizado. URL atual: ${page.url()}`);
    }

    await runSceneSteps(page, scene, runId);

    if (scene.beforeWaitSeconds) {
      await waitSeconds(page, scene.beforeWaitSeconds);
    }

    const screenshotPath = scene.fullPageScreenshotPath
      ? path.resolve(scene.fullPageScreenshotPath)
      : path.join(outputDir, `scene-${String(sceneIndex).padStart(2, "0")}.png`);

    if (screenshotPath) {
      ensureDirectory(path.dirname(screenshotPath));
      await page.screenshot({
        path: screenshotPath,
        fullPage: true
      });
      log(`Cena ${sceneIndex}: screenshot salvo em ${screenshotPath}`);
    }

    await waitSeconds(page, scene.durationSeconds ?? 4);
  } catch (error) {
    const failureScreenshotPath = path.join(outputDir, `${sceneSlug}-failure.png`);
    const failureHtmlPath = path.join(outputDir, `${sceneSlug}-failure.html`);
    const failureMetaPath = path.join(outputDir, `${sceneSlug}-failure.txt`);

    ensureDirectory(path.dirname(failureScreenshotPath));
    await page.screenshot({ path: failureScreenshotPath, fullPage: true }).catch(() => {});
    fs.writeFileSync(failureHtmlPath, await page.content().catch(() => ""), "utf8");
    fs.writeFileSync(
      failureMetaPath,
      [
        `scene=${scene.title}`,
        `targetUrl=${targetUrl}`,
        `currentUrl=${page.url()}`,
        `error=${error?.stack ?? error?.message ?? String(error)}`
      ].join("\n"),
      "utf8"
    );
    log(`Cena ${sceneIndex}: falhou em ${page.url()}`);
    log(`Cena ${sceneIndex}: evidencias salvas em ${failureScreenshotPath}`);
    throw error;
  }
}

async function main() {
  const args = readArgs(process.argv.slice(2));
  const manifestPath = args.manifest;
  const screenshotDir = args.screenshotDir;
  const rawVideoPath = args.rawVideo;

  if (!manifestPath) {
    throw new Error("Parametro obrigatorio ausente: --manifest");
  }

  if (!screenshotDir || !rawVideoPath) {
    throw new Error("Parametros obrigatorios ausentes: --screenshotDir e --rawVideo");
  }

  const manifest = loadManifest(manifestPath);
  const runId = new Date().toISOString().replace(/[-:.TZ]/g, "").slice(0, 14);
  const baseUrl = manifest.baseUrl;
  if (!baseUrl) {
    throw new Error("O manifesto web precisa informar baseUrl.");
  }

  const width = manifest.video?.width ?? 1920;
  const height = manifest.video?.height ?? 1080;
  ensureDirectory(screenshotDir);

  const browser = await chromium.launch({
    headless: Boolean(manifest.playwright?.headless)
  });
  log(`Chromium iniciado. Headless: ${Boolean(manifest.playwright?.headless)}`);

  const context = await browser.newContext({
    viewport: { width, height },
    recordVideo: { dir: screenshotDir, size: { width, height } }
  });

  const page = await context.newPage();
  let currentAuthProfile = null;

  let sceneIndex = 1;
  for (const scene of manifest.scenes ?? []) {
    const sceneAuthProfile = scene.authProfile;
    if (!sceneAuthProfile) {
      throw new Error(`Cena sem authProfile definida: ${scene.title}`);
    }

    if (currentAuthProfile !== sceneAuthProfile) {
      await performLogin(page, manifest, sceneAuthProfile);
      currentAuthProfile = sceneAuthProfile;
    }

    await captureScene(page, scene, baseUrl, screenshotDir, sceneIndex, runId);
    sceneIndex += 1;
  }

  const video = page.video();
  log("Fechando contexto e navegador para finalizar o video bruto.");
  await context.close();
  await browser.close();
  const recordedVideoPath = await video.path();

  ensureDirectory(path.dirname(rawVideoPath));
  fs.copyFileSync(recordedVideoPath, rawVideoPath);
  log(`Video bruto copiado para ${rawVideoPath}`);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
