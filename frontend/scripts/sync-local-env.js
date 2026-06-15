/**
 * Writes the machine's current LAN IPv4 into .env.local so a physical device
 * on the same WiFi can reach the Metro bundler and the local API without any
 * hand-editing. Runs automatically on `npm start` (prestart hook) and only
 * does anything when the target environment is `local`.
 *
 * It sets:
 *   - REACT_NATIVE_PACKAGER_HOSTNAME  -> Metro binds to the LAN IP
 *   - EXPO_PUBLIC_LOCAL_API_HOST      -> lib/env.ts builds http://<ip>:5180
 *
 * .env.local is gitignored and has higher precedence than .env.development.
 */
const fs = require('fs');
const os = require('os');
const path = require('path');

const ENV_FILE = path.resolve(__dirname, '..', '.env.local');
const SYNCED_KEYS = ['REACT_NATIVE_PACKAGER_HOSTNAME', 'EXPO_PUBLIC_LOCAL_API_HOST'];
const IGNORED_INTERFACE_NAME =
  /(loopback|bluetooth|virtual|vmware|virtualbox|vbox|hyper-v|wsl|docker|vethernet)/i;

function syncLocalEnv(options = {}) {
  const appEnv = process.env.EXPO_PUBLIC_APP_ENV || readEnvFile(ENV_FILE).EXPO_PUBLIC_APP_ENV || 'local';
  if (appEnv !== 'local') {
    return null;
  }

  const localIp = findLocalIpAddress();
  if (!localIp) {
    if (!options.silent) {
      console.warn('sync-local-env: no active LAN IPv4 found; using platform default API URL.');
    }
    return null;
  }

  const updated = upsertEnvValues(ENV_FILE, {
    REACT_NATIVE_PACKAGER_HOSTNAME: localIp,
    EXPO_PUBLIC_LOCAL_API_HOST: localIp,
  });

  process.env.REACT_NATIVE_PACKAGER_HOSTNAME = localIp;
  process.env.EXPO_PUBLIC_LOCAL_API_HOST = localIp;

  if (!options.silent && updated) {
    console.log(`sync-local-env: local host set to ${localIp}.`);
  }

  return localIp;
}

function findLocalIpAddress() {
  const candidates = [];

  for (const [name, addresses] of Object.entries(os.networkInterfaces())) {
    if (!addresses || IGNORED_INTERFACE_NAME.test(name)) {
      continue;
    }
    for (const address of addresses) {
      if (address.family !== 'IPv4' || address.internal || !isPrivateIp(address.address)) {
        continue;
      }
      candidates.push({ address: address.address, score: scoreInterface(name, address.address) });
    }
  }

  candidates.sort((left, right) => right.score - left.score);
  return candidates[0]?.address ?? null;
}

function scoreInterface(name, address) {
  let score = 0;
  if (/wi-?fi|wireless|wlan/i.test(name)) score += 50;
  if (/ethernet/i.test(name)) score += 30;
  if (address.startsWith('192.168.')) score += 20;
  else if (address.startsWith('10.')) score += 10;
  else if (/^172\.(1[6-9]|2\d|3[0-1])\./.test(address)) score += 10;
  return score;
}

function isPrivateIp(address) {
  return (
    address.startsWith('10.') ||
    address.startsWith('192.168.') ||
    /^172\.(1[6-9]|2\d|3[0-1])\./.test(address)
  );
}

function readEnvFile(filePath) {
  if (!fs.existsSync(filePath)) {
    return {};
  }
  return fs.readFileSync(filePath, 'utf8').split(/\r?\n/).reduce((values, line) => {
    const match = line.match(/^\s*([\w.-]+)\s*=\s*(.*)\s*$/);
    if (match) {
      values[match[1]] = match[2];
    }
    return values;
  }, {});
}

function upsertEnvValues(filePath, values) {
  const existing = fs.existsSync(filePath) ? fs.readFileSync(filePath, 'utf8') : '';
  const hasTrailingNewline = existing.endsWith('\n') || existing.length === 0;
  const lines = existing.length > 0 ? existing.split(/\r?\n/) : [];
  const seen = new Set();
  let changed = false;

  const updatedLines = lines.map((line) => {
    const match = line.match(/^(\s*([\w.-]+)\s*=\s*)(.*)$/);
    if (!match || !Object.prototype.hasOwnProperty.call(values, match[2])) {
      return line;
    }
    seen.add(match[2]);
    if (match[3] === values[match[2]]) {
      return line;
    }
    changed = true;
    return `${match[1]}${values[match[2]]}`;
  });

  for (const key of SYNCED_KEYS) {
    if (!seen.has(key)) {
      updatedLines.push(`${key}=${values[key]}`);
      changed = true;
    }
  }

  if (!changed) {
    return false;
  }

  fs.writeFileSync(filePath, `${updatedLines.join(os.EOL)}${hasTrailingNewline ? '' : os.EOL}`);
  return true;
}

if (require.main === module) {
  syncLocalEnv();
}

module.exports = { syncLocalEnv };
