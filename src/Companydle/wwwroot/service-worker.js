// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
//
// The development worker still takes over immediately and accepts the same
// SKIP_WAITING message as the published worker. This lets a browser replace a
// previously installed published worker without getting stuck on the update
// banner or serving old cached assets.

self.addEventListener('install', () => {
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    event.waitUntil((async () => {
        // Published builds use this cache prefix for offline support. It is
        // not used in development, so clear it when the development worker
        // takes over an origin that was previously used for a published build.
        const cacheKeys = await caches.keys();
        await Promise.all(
            cacheKeys
                .filter(key => key.startsWith('companydle-offline-cache-'))
                .map(key => caches.delete(key)));

        await self.clients.claim();
    })());
});

self.addEventListener('message', event => {
    if (event.data === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

self.addEventListener('fetch', () => { });
