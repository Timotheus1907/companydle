(function () {
    if (!('serviceWorker' in navigator)) {
        return;
    }

    let registration = null;
    let updateCallback = null;
    let updateReady = false;
    let reloading = false;
    let hadController = !!navigator.serviceWorker.controller;

    function notifyUpdate() {
        updateReady = true;
        if (updateCallback) {
            updateCallback.invokeMethodAsync('OnUpdateAvailable')
                .catch(function () {
                    // The component may have been disposed during navigation.
                });
        }
    }

    window.companydleWatchForUpdates = function (dotnetCallback) {
        updateCallback = dotnetCallback;
        return updateReady;
    };

    window.companydleUnwatchForUpdates = function () {
        updateCallback = null;
    };

    function reloadPage() {
        if (reloading) {
            return;
        }

        reloading = true;
        location.reload();
    }

    window.companydleApplyUpdate = function () {
        const waiting = registration && registration.waiting;

        if (!waiting) {
            reloadPage();
            return;
        }

        if (waiting.state === 'activated') {
            reloadPage();
            return;
        }

        // Wait until the new worker has actually activated before reloading.
        // Reloading on a timer can beat a slow activation and bring the banner
        // back because the old worker is still in control.
        waiting.addEventListener('statechange', function () {
            if (waiting.state === 'activated') {
                reloadPage();
            }
        });

        waiting.postMessage('SKIP_WAITING');
    };

    navigator.serviceWorker.addEventListener('controllerchange', function () {
        if (!hadController) {
            // The first service worker claimed this page after its first load.
            // No reload is needed on that initial install.
            hadController = true;
            return;
        }

        reloadPage();
    });

    (async function () {
        try {
            registration = await navigator.serviceWorker.register(
                new URL('service-worker.js', document.baseURI),
                { updateViaCache: 'none' });
            window.__companydleRegistration = registration;

            if (registration.waiting && hadController) {
                notifyUpdate();
            }

            registration.addEventListener('updatefound', function () {
                const installing = registration.installing;
                if (!installing) {
                    return;
                }

                installing.addEventListener('statechange', function () {
                    if (installing.state === 'installed' && hadController) {
                        notifyUpdate();
                    }
                });
            });

            document.addEventListener('visibilitychange', function () {
                if (document.visibilityState === 'visible') {
                    registration.update().catch(function () {
                        // Offline or a transient network failure; try again later.
                    });
                }
            });
        } catch (error) {
            console.warn('Companydle: service worker registration failed.', error);
        }
    })();
})();
