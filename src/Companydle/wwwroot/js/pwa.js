(function () {
    if (!('serviceWorker' in navigator)) {
        return;
    }

    let registration = null;
    let updateCallback = null;
    let updateReady = false;
    let reloading = false;

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
            return;
        }

        location.reload();
    };

    navigator.serviceWorker.addEventListener('controllerchange', function () {
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

            if (registration.waiting && navigator.serviceWorker.controller) {
                notifyUpdate();
            }

            registration.addEventListener('updatefound', function () {
                const installing = registration.installing;
                if (!installing) {
                    return;
                }

                installing.addEventListener('statechange', function () {
                    if (installing.state === 'installed' && navigator.serviceWorker.controller) {
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
