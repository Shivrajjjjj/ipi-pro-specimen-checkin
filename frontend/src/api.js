import axios from 'axios';

const api = axios.create({
    baseURL: 'http://localhost:5000/api',
    headers: {
        'X-Lab-Id': '11111111-1111-1111-1111-111111111111'
    }
});

export default api;