import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    vus: 1,
    iterations: 5,
};

export function setup() {
    const loginPayload = JSON.stringify({
        username: 'admin',
        password: 'password',
    });

    const loginParams = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const loginResponse = http.post(
        'http://localhost:5241/api/auth/login',
        loginPayload,
        loginParams
    );

    check(loginResponse, {
        'login successful': (response) => response.status === 200,
    });

    return {
        token: loginResponse.json('token'),
    };
}

export default function (data) {
    const params = {
        headers: {
            Authorization: `Bearer ${data.token}`,
        },
    };

    const response = http.get(
        'http://localhost:5241/api/appointments',
        params
    );

    check(response, {
        'appointments request successful': (response) => response.status === 200,
    });

    sleep(1);
}
