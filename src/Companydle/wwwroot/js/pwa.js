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

    window.companydleApplyUpdate = function () {
        if (registration && registration.waiting) {
            registration.waiting.postMessage('SKIP_WAITING');

            // controllerchange normally reloads first. This fallback covers
            // browsers that take a little longer to hand over control.
            setTimeout(function () {
                if (!reloading) {
                    reloading = true;
                    location.reload();
                }
            }, 1000);
            return;
        }

        location.reload();
    };

    navigator.serviceWorker.addEventListener('controllerchange', function () {
        if (!hadController) {
            // The first service worker claimed this page after its first load.
            // No reload is needed on that initial install.
            hadController = true;
            return;
        }

        if (reloading) {
            return;
        }

        reloading = true;
        location.reload();
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
