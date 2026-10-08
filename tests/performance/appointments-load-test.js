import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5241';

export const options = {
    stages: [
        { duration: '10s', target: 2 },
        { duration: '20s', target: 2 },
        { duration: '10s', target: 5 },
        { duration: '20s', target: 5 },
        { duration: '10s', target: 0 },
    ],
    thresholds: {
        http_req_failed: ['rate<0.05'],
        http_req_duration: ['p(95)<1000'],
    },
};

export function setup() {
    const response = http.post(
        `${BASE_URL}/api/auth/login`,
        JSON.stringify({
            username: 'admin',
            password: 'password',
        }),
        {
            headers: {
                'Content-Type': 'application/json',
            },
        }
    );

    check(response, {
        'login successful': (r) => r.status === 200,
        'login returned token': (r) => {
            const body = r.json();
            return body && body.token;
        },
    });

    if (response.status !== 200) {
        throw new Error(`Login failed with status ${response.status}`);
    }

    const body = response.json();

    if (!body.token) {
        throw new Error('Login response did not contain a token');
    }

    return {
        token: body.token,
    };
}

export default function (data) {
    const response = http.get(
        `${BASE_URL}/api/appointments`,
        {
            headers: {
                Authorization: `Bearer ${data.token}`,
            },
        }
    );

    check(response, {
        'appointments request successful': (r) => r.status === 200,
    });

    sleep(1);
}