import { computed, onBeforeUnmount, onMounted, ref } from "vue";

interface BeforeInstallPromptEvent extends Event {
  prompt: () => Promise<void>;
  userChoice: Promise<{ outcome: "accepted" | "dismissed" }>;
}
function isIosDevice(): boolean {
  return (
    /iPad|iPhone|iPod/.test(navigator.userAgent) || (navigator.platform === "MacIntel" && navigator.maxTouchPoints > 1)
  );
}

function isStandaloneDisplay(): boolean {
  return (
    window.matchMedia("(display-mode: standalone)").matches ||
    ("standalone" in navigator && (navigator as Navigator & { standalone?: boolean }).standalone === true)
  );
}

export function usePwaInstall() {
  const installPrompt = ref<BeforeInstallPromptEvent>();
  const installed = ref(false);
  const instructionsOpen = ref(false);
  const ios = ref(false);
  const nativePromptAvailable = computed(() => installPrompt.value !== undefined);

  function handleBeforeInstallPrompt(event: Event): void {
    event.preventDefault();
    installPrompt.value = event as BeforeInstallPromptEvent;
  }

  function handleAppInstalled(): void {
    installed.value = true;
    installPrompt.value = undefined;
    instructionsOpen.value = false;
  }

  async function install(): Promise<void> {
    if (installed.value) {
      return;
    }

    if (!installPrompt.value) {
      instructionsOpen.value = true;

      return;
    }

    const prompt = installPrompt.value;
    await prompt.prompt();
    const choice = await prompt.userChoice;

    if (choice.outcome === "accepted") {
      installPrompt.value = undefined;
    }
  }

  onMounted(() => {
    ios.value = isIosDevice();
    installed.value = isStandaloneDisplay();
    window.addEventListener("beforeinstallprompt", handleBeforeInstallPrompt);
    window.addEventListener("appinstalled", handleAppInstalled);
  });

  onBeforeUnmount(() => {
    window.removeEventListener("beforeinstallprompt", handleBeforeInstallPrompt);
    window.removeEventListener("appinstalled", handleAppInstalled);
  });

  return {
    install,
    installed,
    instructionsOpen,
    ios,
    nativePromptAvailable,
  };
}
