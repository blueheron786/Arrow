window.arrowAuth = (() => {
    const antiforgeryCookieName = 'XSRF-TOKEN';
    let tokenCache = null;

    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) {
            return parts.pop()?.split(';').shift() ?? '';
        }
        return '';
    }

    function getAntiforgeryToken() {
        if (!tokenCache) {
            tokenCache = getCookie(antiforgeryCookieName);
        }
        return tokenCache;
    }

    async function ensureAntiforgeryToken() {
        const token = getAntiforgeryToken();
        if (token) {
            return token;
        }

        // Fetch the antiforgery token
        try {
            await fetch('/antiforgery/token', {
                method: 'GET',
                credentials: 'include'
            });
            tokenCache = getCookie(antiforgeryCookieName);
            return tokenCache;
        } catch (error) {
            console.error('Failed to fetch antiforgery token:', error);
            return null;
        }
    }

    async function postJson(path, payload) {
        const token = await ensureAntiforgeryToken();
        const response = await fetch(path, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                ...(token ? { 'RequestVerificationToken': token } : {})
            },
            body: payload ? JSON.stringify(payload) : null
        });

        const text = await response.text();
        if (!text) {
            return { succeeded: response.ok, errors: response.ok ? [] : ['Unknown error'] };
        }

        return JSON.parse(text);
    }

    return {
        register: payload => postJson('/auth/register', payload),
        login: payload => postJson('/auth/login', payload),
        logout: () => postJson('/auth/logout')
    };
})();
