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

function loadManifest(manifestPath) {
  const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"));
  const slugMap = {
    amigos: process.env.VIDEO_SLUG_AMIGOS || "amigos-da-pesca-2026",
    rei: process.env.VIDEO_SLUG_REI || "rei-dos-mares-2026",
    bts: process.env.VIDEO_SLUG_BTS || "bts-sport-fishing-2026"
  };

  if (manifest.authProfiles) {
    for (const [profileName, profile] of Object.entries(manifest.authProfiles)) {
      const envPrefix = `VIDEO_APP_${profileName.toUpperCase().replace(/[^A-Z0-9]/g, "_")}`;
      profile.username = process.env[`${envPrefix}_USERNAME`] || profile.username;
      profile.password = process.env[`${envPrefix}_PASSWORD`] || profile.password;
      profile.slug = replaceSlugTokens(process.env[`${envPrefix}_SLUG`] || profile.slug, slugMap);
      profile.profile = process.env[`${envPrefix}_PROFILE`] || profile.profile;
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

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function startScreenRecord(outputPath, maxSeconds) {
  const remotePath = "/sdcard/torneio-video-recording.mp4";
  const child = spawn("adb", [
    "shell",
    "screenrecord",
    "--time-limit",
    String(maxSeconds),
    remotePath
  ], {
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
  const selector = `android=new UiSelector().text("${text}")`;
  await driver.$(selector).waitForDisplayed({ timeout: timeoutMs });
}

async function tapText(driver, text) {
  const selector = `android=new UiSelector().text("${text}")`;
  const element = await driver.$(selector);
  await element.waitForDisplayed({ timeout: 20000 });
  await element.click();
}

async function setFirstEditText(driver, index, value) {
  const selector = `android=new UiSelector().className("android.widget.EditText").instance(${index})`;
  const element = await driver.$(selector);
  await element.waitForDisplayed({ timeout: 20000 });
  await element.setValue(value);
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

async function performLogin(driver, manifest, authProfileName) {
  const auth = manifest.authProfiles?.[authProfileName];
  if (!auth) {
    throw new Error(`Perfil de autenticacao do app nao encontrado: ${authProfileName}`);
  }

  await clearAppData(manifest.app?.packageName ?? "com.example.torneio_app");

  await openDeepLink(auth.slug);
  await waitForText(driver, auth.tournamentName);
  await tapText(driver, "Fiscal/Administracao");
  await waitForText(driver, "Entrar");

  if (auth.profile === "Admin") {
    await tapText(driver, "Admin");
  } else {
    await tapText(driver, auth.fiscalLabel ?? "Fiscal");
  }

  await setFirstEditText(driver, 0, auth.username);
  await setFirstEditText(driver, 1, auth.password);
  await tapText(driver, "Entrar");

  if (auth.expectedHomeText) {
    await waitForText(driver, auth.expectedHomeText, 30000);
  }
}

async function captureScene(driver, scene, outputDir, sceneIndex) {
  if (scene.slug) {
    await openDeepLink(scene.slug);
    await sleep(3000);
  }

  if (scene.tapText) {
    await tapText(driver, scene.tapText);
  }

  if (scene.waitForText) {
    await waitForText(driver, scene.waitForText);
  }

  const screenshotPath = path.join(outputDir, `scene-${String(sceneIndex).padStart(2, "0")}.png`);
  await driver.saveScreenshot(screenshotPath);
  await sleep((scene.durationSeconds ?? 4) * 1000);
}

async function main() {
  const args = readArgs(process.argv.slice(2));
  const manifestPath = args.manifest;
  const outputDir = args.outputDir;
  const rawVideoPath = args.rawVideo;
  const appiumHost = args.host ?? "127.0.0.1";
  const appiumPort = Number(args.port ?? "4723");

  if (!manifestPath || !outputDir || !rawVideoPath) {
    throw new Error("Parametros obrigatorios: --manifest, --outputDir, --rawVideo");
  }

  const manifest = loadManifest(manifestPath);
  ensureDirectory(outputDir);
  ensureDirectory(path.dirname(rawVideoPath));

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
        throw new Error(`Cena sem authProfile definida: ${scene.title}`);
      }

      if (currentAuthProfile !== sceneAuthProfile) {
        await performLogin(driver, manifest, sceneAuthProfile);
        currentAuthProfile = sceneAuthProfile;
      }

      await captureScene(driver, scene, outputDir, sceneIndex);
      sceneIndex += 1;
    }
  } finally {
    await driver.deleteSession();
    await screenRecord.stop();
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
