import fs from "node:fs";
import path from "node:path";
import process from "node:process";
import { execFileSync, spawn } from "node:child_process";
import { remote } from "webdriverio";

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

function writeJsonFile(filePath, value) {
  ensureDirectory(path.dirname(filePath));
  fs.writeFileSync(filePath, JSON.stringify(value, null, 2));
}

function readJsonFile(filePath) {
  const rawContent = fs.readFileSync(filePath, "utf8").replace(/^\uFEFF/, "");
  return JSON.parse(rawContent);
}

function loadManifest(manifestPath) {
  const manifest = readJsonFile(manifestPath);
  const slugMap = {
    amigos: process.env.VIDEO_SLUG_AMIGOS || "amigos-da-pesca-2026",
    rei: process.env.VIDEO_SLUG_REI || "rei-dos-mares-2026",
    bts: process.env.VIDEO_SLUG_BTS || "bts-sport-fishing-2026"
  };

  if (manifest.authProfiles) {
    for (const [profileName, profile] of Object.entries(manifest.authProfiles)) {
      const envPrefixCandidates = [
        `VIDEO_APP_${profileName.toUpperCase().replace(/[^A-Z0-9]/g, "_")}`,
        `VIDEO_APP_${profileName
          .replace(/_(HOME|REGISTRAR|SYNC|EMBARCACOES|PESCADORES|PEIXES|FISCAIS|CAPTURAS|SORTEIO|RELATORIOS|REORGANIZACAO)$/i, "")
          .toUpperCase()
          .replace(/[^A-Z0-9]/g, "_")}`
      ];

      const resolveProfileEnv = (suffix) => {
        for (const prefix of envPrefixCandidates) {
          const value = process.env[`${prefix}_${suffix}`];
          if (value) {
            return value;
          }
        }

        return null;
      };

      profile.username = resolveProfileEnv("USERNAME") || profile.username;
      profile.password = resolveProfileEnv("PASSWORD") || profile.password;
      profile.slug = replaceSlugTokens(resolveProfileEnv("SLUG") || profile.slug, slugMap);
      profile.profile = resolveProfileEnv("PROFILE") || profile.profile;
    }
  }

  if (Array.isArray(manifest.scenes)) {
    for (const scene of manifest.scenes) {
      if (scene.slug) {
        scene.slug = replaceSlugTokens(scene.slug, slugMap);
      }
    }
  }

  return manifest;
}

function replaceSlugTokens(value, slugMap) {
  if (!value || typeof value !== "string") {
    return value;
  }

  return value
    .replaceAll("{slug_amigos}", slugMap.amigos)
    .replaceAll("{slug_rei}", slugMap.rei)
    .replaceAll("{slug_bts}", slugMap.bts);
}

function adb(args) {
  const adbPath = process.env.VIDEO_ADB_PATH || "adb";
  const serial = process.env.VIDEO_ADB_SERIAL;
  const fullArgs = serial ? ["-s", serial, ...args] : args;
  return execFileSync(adbPath, fullArgs, { encoding: "utf8" }).trim();
}

let cachedDisplaySize = null;

function getDisplaySize() {
  if (cachedDisplaySize) {
    return cachedDisplaySize;
  }

  const raw = adb(["shell", "wm", "size"]);
  const match = raw.match(/Physical size:\s*(\d+)x(\d+)/i) ?? raw.match(/Override size:\s*(\d+)x(\d+)/i);
  if (!match) {
    throw new Error(`Nao foi possivel identificar o tamanho da tela: ${raw}`);
  }

  cachedDisplaySize = {
    width: Number(match[1]),
    height: Number(match[2])
  };
  return cachedDisplaySize;
}

function escapeAdbInputText(value) {
  return String(value)
    .replaceAll("\\", "\\\\")
    .replaceAll(" ", "%s")
    .replaceAll("&", "\\&")
    .replaceAll("<", "\\<")
    .replaceAll(">", "\\>")
    .replaceAll("(", "\\(")
    .replaceAll(")", "\\)")
    .replaceAll(";", "\\;")
    .replaceAll("|", "\\|");
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function tapPercent(xPercent, yPercent) {
  const { width, height } = getDisplaySize();
  const x = Math.round((xPercent / 100) * width);
  const y = Math.round((yPercent / 100) * height);
  adb(["shell", "input", "tap", String(x), String(y)]);
}

function escapeUiAutomatorText(value) {
  return String(value).replaceAll("\\", "\\\\").replaceAll("\"", "\\\"");
}

function stripDiacritics(value) {
  return String(value)
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "");
}

function buildSearchTerms(text) {
  const raw = String(text).trim();
  const ascii = stripDiacritics(raw);
  const prefixLengths = [raw.length, 24, 18, 14, 10];
  const values = new Set();

  for (const source of [raw, ascii]) {
    const normalized = source.trim();
    if (!normalized) {
      continue;
    }

    values.add(normalized);
    for (const length of prefixLengths) {
      if (normalized.length >= length) {
        values.add(normalized.slice(0, length).trim());
      }
    }
  }

  return [...values].filter(Boolean);
}

function startScreenRecord(outputPath, maxSeconds) {
  const remotePath = "/sdcard/torneio-video-recording.mp4";
  const adbPath = process.env.VIDEO_ADB_PATH || "adb";
  const serial = process.env.VIDEO_ADB_SERIAL;
  const fullArgs = [
    ...(serial ? ["-s", serial] : []),
    "shell",
    "screenrecord",
    "--time-limit",
    String(maxSeconds),
    remotePath
  ];
  const child = spawn(adbPath, fullArgs, {
    stdio: "ignore"
  });

  return {
    remotePath,
    child,
    async stop() {
      if (!child.killed) {
        child.kill("SIGINT");
      }

      await sleep(2000);
      ensureDirectory(path.dirname(outputPath));
      adb(["pull", remotePath, outputPath]);
      adb(["shell", "rm", remotePath]);
    }
  };
}

async function waitForText(driver, text, timeoutMs = 20000) {
  const selectors = [];
  for (const term of buildSearchTerms(text)) {
    const escapedText = escapeUiAutomatorText(term);
    selectors.push(
      `android=new UiSelector().text("${escapedText}")`,
      `android=new UiSelector().textContains("${escapedText}")`,
      `android=new UiSelector().description("${escapedText}")`,
      `android=new UiSelector().descriptionContains("${escapedText}")`
    );
  }

  const deadline = Date.now() + timeoutMs;
  let lastError = null;

  while (Date.now() < deadline) {
    for (const selector of selectors) {
      try {
        const element = await driver.$(selector);
        if (await element.isDisplayed()) {
          return element;
        }
      } catch (error) {
        lastError = error;
      }
    }

    await sleep(500);
  }

  throw lastError ?? new Error(`Texto nao encontrado: ${text}`);
}

async function tapText(driver, text) {
  const element = await waitForText(driver, text, 20000);
  await element.click();
}

async function setFirstEditText(driver, index, value) {
  const selector = `android=new UiSelector().className("android.widget.EditText").instance(${index})`;
  const element = await driver.$(selector);
  await element.waitForDisplayed({ timeout: 20000 });
  await element.click();
  await sleep(300);
  try {
    await element.clearValue();
  } catch {
    // Alguns campos nao suportam clearValue de forma consistente no Android.
  }
  await sleep(300);
  adb(["shell", "input", "text", escapeAdbInputText(value)]);
  await sleep(500);
}

async function openDeepLink(slug) {
  adb([
    "shell",
    "am",
    "start",
    "-W",
    "-a",
    "android.intent.action.VIEW",
    "-d",
    `https://torneio.ari.net.br/${slug}`,
    "com.example.torneio_app"
  ]);
}

async function clearAppData(packageName) {
  adb(["shell", "pm", "clear", packageName]);
}

async function launchAppHome(manifest) {
  const packageName = manifest.app?.packageName ?? "com.example.torneio_app";
  const activityName = manifest.app?.activityName ?? ".MainActivity";
  adb([
    "shell",
    "am",
    "start",
    "-W",
    "-n",
    `${packageName}/${activityName}`
  ]);
  await sleep(3000);
}

async function performLogin(driver, manifest, authProfileName) {
  const auth = manifest.authProfiles?.[authProfileName];
  if (!auth) {
    throw new Error(`Perfil de autenticacao do app nao encontrado: ${authProfileName}`);
  }

  await clearAppData(manifest.app?.packageName ?? "com.example.torneio_app");

  await openDeepLink(auth.slug);
  await waitForText(driver, auth.tournamentName);
  tapPercent(50, 56.7);
  await sleep(1200);
  await waitForText(driver, "Entrar");

  if (auth.profile === "Admin") {
    tapPercent(61.5, 15.8);
    await sleep(500);
  }

  await setFirstEditText(driver, 0, auth.username);
  await setFirstEditText(driver, 1, auth.password);
  tapPercent(51, 47.9);
  await sleep(2000);

  if (auth.expectedHomeText) {
    await waitForText(driver, auth.expectedHomeText, 30000);
  }
}

async function executeSceneActions(driver, scene) {
  for (const action of scene.actions ?? []) {
    switch (action.type) {
      case "tapPercent":
        tapPercent(action.x, action.y);
        await sleep(action.delayMs ?? 1200);
        break;
      case "tapText":
        await tapText(driver, action.text);
        await sleep(action.delayMs ?? 1200);
        break;
      case "waitText":
        await waitForText(driver, action.text, action.timeoutMs ?? 20000);
        break;
      case "sleep":
        await sleep(action.ms ?? 1000);
        break;
      case "back":
        adb(["shell", "input", "keyevent", "4"]);
        await sleep(action.delayMs ?? 1000);
        break;
      default:
        throw new Error(`Acao de cena nao suportada: ${action.type}`);
    }
  }
}

async function captureScene(driver, scene, outputDir, sceneIndex, targetDurationSeconds, recordingStartedAt, sceneTimingEntries) {
  const sceneStartedAt = (Date.now() - recordingStartedAt) / 1000;

  if (scene.slug) {
    await openDeepLink(scene.slug);
    await sleep(3000);
  }

  await executeSceneActions(driver, scene);

  if (scene.tapText) {
    await tapText(driver, scene.tapText);
  }

  if (scene.waitForText) {
    await waitForText(driver, scene.waitForText);
  }

  const screenshotPath = path.join(outputDir, `scene-${String(sceneIndex).padStart(2, "0")}.png`);
  await driver.saveScreenshot(screenshotPath);

  const elapsedSceneSeconds = (Date.now() - recordingStartedAt) / 1000 - sceneStartedAt;
  const remainingSeconds = Math.max(targetDurationSeconds - elapsedSceneSeconds, 0.5);
  await sleep(remainingSeconds * 1000);

  const sceneEndedAt = (Date.now() - recordingStartedAt) / 1000;
  sceneTimingEntries.push({
    index: sceneIndex,
    title: scene.title,
    startSeconds: Number(sceneStartedAt.toFixed(3)),
    endSeconds: Number(sceneEndedAt.toFixed(3))
  });
}

async function main() {
  const args = readArgs(process.argv.slice(2));
  const manifestPath = args.manifest;
  const outputDir = args.outputDir;
  const rawVideoPath = args.rawVideo;
  const timingMetadataPath = args.timingMetadata;
  const voiceMetadataPath = args.voiceMetadata;
  const appiumHost = args.host ?? "127.0.0.1";
  const appiumPort = Number(args.port ?? "4723");

  if (!manifestPath || !outputDir || !rawVideoPath) {
    throw new Error("Parametros obrigatorios: --manifest, --outputDir, --rawVideo");
  }

  const manifest = loadManifest(manifestPath);
  const voiceMetadata = voiceMetadataPath ? readJsonFile(voiceMetadataPath) : null;
  ensureDirectory(outputDir);
  ensureDirectory(path.dirname(rawVideoPath));
  if (timingMetadataPath) {
    ensureDirectory(path.dirname(timingMetadataPath));
  }

  const capabilities = {
    platformName: "Android",
    "appium:automationName": "UiAutomator2",
    "appium:deviceName": manifest.device?.name ?? "Android Emulator",
    "appium:appPackage": manifest.app?.packageName ?? "com.example.torneio_app",
    "appium:appActivity": manifest.app?.activityName ?? ".MainActivity",
    "appium:noReset": true,
    "appium:newCommandTimeout": 240,
    "appium:autoGrantPermissions": true
  };

  const screenRecord = startScreenRecord(rawVideoPath, manifest.recording?.maxSeconds ?? 180);
  const recordingStartedAt = Date.now();
  const sceneTimingEntries = [];

  const driver = await remote({
    hostname: appiumHost,
    port: appiumPort,
    path: "/",
    capabilities
  });

  try {
    let currentAuthProfile = null;
    let sceneIndex = 1;
    for (const scene of manifest.scenes ?? []) {
      const sceneAuthProfile = scene.authProfile;
      if (!sceneAuthProfile) {
        await clearAppData(manifest.app?.packageName ?? "com.example.torneio_app");
        await launchAppHome(manifest);
        currentAuthProfile = null;
      } else if (currentAuthProfile !== sceneAuthProfile) {
        await performLogin(driver, manifest, sceneAuthProfile);
        currentAuthProfile = sceneAuthProfile;
      }

      const targetDurationSeconds = voiceMetadata?.Scenes?.[sceneIndex - 1]?.DurationSeconds
        ?? scene.durationSeconds
        ?? 4;
      await captureScene(
        driver,
        scene,
        outputDir,
        sceneIndex,
        targetDurationSeconds,
        recordingStartedAt,
        sceneTimingEntries
      );
      sceneIndex += 1;
    }
  } finally {
    await driver.deleteSession();
    await screenRecord.stop();
    if (timingMetadataPath) {
      writeJsonFile(timingMetadataPath, {
        rawVideoPath,
        scenes: sceneTimingEntries
      });
    }
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
