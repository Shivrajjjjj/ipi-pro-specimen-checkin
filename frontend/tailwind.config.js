/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{vue,js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                ipiNavy: '#1E293B',
                ipiHeader: '#0F172A',
                ipiBg: '#F8FAFC',
                ipiAccent: '#1E56A0',
                ipiGreen: '#D1FAE5',
                ipiGreenText: '#065F46',
                ipiRed: '#FEE2E2',
                ipiRedText: '#991B1B'
            }
        },
    },
    plugins: [],
}