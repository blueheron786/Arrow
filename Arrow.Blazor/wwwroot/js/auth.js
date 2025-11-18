window.arrowAuth = (() => {
    const antiforgeryCookieName = 'RequestVerificationToken';

    function getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) {
            return parts.pop()?.split(';').shift() ?? '';
        }
        return '';
    }

    function getAntiforgeryToken() {
        return getCookie(antiforgeryCookieName);
    }

    async function postJson(path, payload) {
        const token = getAntiforgeryToken();
        const response = await fetch(path, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'Content-Type': 'application/json',
                ...(token ? { RequestVerificationToken: token } : {})
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
