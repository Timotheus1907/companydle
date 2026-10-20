(function () {
    window.companydleShare = async function (text) {
        if (navigator.share) {
            try {
                await navigator.share({ text: text, title: 'Companydle' });
                return 'shared';
            } catch (error) {
                if (error && error.name === 'AbortError') {
                    return 'cancelled';
                }
            }
        }

        if (navigator.clipboard && navigator.clipboard.writeText) {
            try {
                await navigator.clipboard.writeText(text);
                return 'copied';
            } catch (error) {
                // Fall through to the visible textarea fallback.
            }
        }

        return 'manual';
    };
})();
