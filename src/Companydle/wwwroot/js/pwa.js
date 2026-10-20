(function () {
    if (!('serviceWorker' in navigator)) {
        return;
    }

    const serviceWorkerUrl = new URL('service-worker.js', document.baseURI);

    navigator.serviceWorker.register(serviceWorkerUrl, { updateViaCache: 'none' })
        .catch(function (error) {
            console.warn('Companydle: service worker registration failed.', error);
        });
})();
