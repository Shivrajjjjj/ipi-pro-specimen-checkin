import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost:5052/api',
    headers: {
        'X-Lab-Id': '11111111-1111-1111-1111-111111111111',
        'Content-Type': 'application/json'
    },
    timeout: 10000
});

// Response interceptor for error handling
api.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 404) {
            console.error('Resource not found or unauthorized access');
        } else if (error.response?.status === 400) {
            console.error('Bad request:', error.response.data);
        } else if (error.code === 'ECONNABORTED') {
            console.error('Request timeout');
        }
        return Promise.reject(error);
    }
);

export default api;