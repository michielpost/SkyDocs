import { AuthClient } from "@dfinity/auth-client";
import { canisterId, createActor, storage } from "../../Dfinity.declarations/storage";
import { ActorSubclass } from "@dfinity/agent";
import { _SERVICE } from "../../Dfinity.declarations/storage/storage.did";

let authClient: AuthClient;
let storage_noauth: ActorSubclass<_SERVICE>;


const init = async () => {
  authClient = await AuthClient.create();
  if (await authClient.isAuthenticated()) {
    handleAuthenticated(authClient);
  }

  storage_noauth = createActor(canisterId as string, {
    agentOptions: {
      host: 'https://ic0.app',
    },
  });
};

let storage_actor: ActorSubclass<_SERVICE>;

async function handleAuthenticated(authClient: AuthClient) {
  const identity = await authClient.getIdentity();
  storage_actor = createActor(canisterId as string, {
    agentOptions: {
      host: 'https://ic0.app',
      identity,
    },
  });

  console.log('login!');
}

init();

export function test() {
  console.log('test');
}

export async function login() {
  await authClient.login({
    onSuccess: async () => {
      handleAuthenticated(authClient);
    },
    identityProvider:
      process.env.DFX_NETWORK === "ic"
        ? "https://identity.ic0.app/#authorize"
        : process.env.LOCAL_II_CANISTER,
  });
}

export async function setValue(key, value) {
  await storage_noauth.insert(key, value);
}

export async function getValue(key) {
  let v = await storage_noauth.lookup(key);
  console.log(v);
  return v;
}

export async function setValueForUser(key, value) {
  await storage_actor.insert(key, value);
}

export async function getValueForUser(key) {
  let v = await storage_actor.lookup(key);
  console.log(v);
  return v;
}

export function isLoggedIn()
{
  var loggedIn = storage_actor != null;
  console.log(loggedIn);
  return loggedIn;
}

export async function logout() {
  await authClient.logout();   
  storage_actor = null!;   
  console.log('logout');
}

export async function whoami() {
  var r = await storage_noauth.whoami();      
  console.log(r);
  return r;
}