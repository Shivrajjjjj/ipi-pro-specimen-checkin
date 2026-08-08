<template>
    <div class="min-h-screen bg-[#F0F4F8] text-slate-800 font-sans text-xs">
        <!-- Top Navigation Header -->
        <header class="bg-[#1C3644] text-white px-6 py-2.5 flex items-center justify-between shadow-sm">
            <div class="flex items-center space-x-4">
                <div class="bg-white text-[#1C3644] font-black px-2 py-0.5 rounded text-sm tracking-wide">IPI</div>
                <span class="border border-slate-400/50 text-slate-200 text-[10px] px-1.5 py-0.2 rounded font-semibold uppercase tracking-wider">UAT</span>
                <div class="text-slate-300 text-xs">
                    Mode: <span class="text-white font-semibold">Check-In</span>
                    <span class="mx-3 text-slate-500">|</span>
                    Location: <span class="text-white font-semibold">Central Lab — Receiving</span>
                </div>
            </div>
            <div class="flex items-center space-x-2 text-xs">
                <span class="text-slate-200 font-medium">Lab Tech 1</span>
                <div class="w-7 h-7 rounded-full bg-[#3B82F6] text-white flex items-center justify-center font-bold text-xs shadow-inner">LT</div>
            </div>
        </header>

        <!-- Sub-Header Tabs -->
        <nav class="bg-white border-b border-slate-200 px-6 flex space-x-6 text-xs font-semibold text-slate-600">
            <button class="text-[#2B6CB0] border-b-2 border-[#2B6CB0] py-2.5 px-1 font-bold">Check-In</button>
            <button class="hover:text-slate-900 py-2.5 px-1 flex items-center space-x-1.5">
                <span>Scan History</span>
                <span class="bg-slate-100 text-slate-600 border border-slate-200 px-1.5 py-0.2 rounded-full text-[10px]">12</span>
            </button>
            <button class="hover:text-slate-900 py-2.5 px-1">Manifests</button>
            <button class="hover:text-slate-900 py-2.5 px-1 flex items-center space-x-1.5">
                <span>Discrepancies</span>
                <span class="bg-red-100 text-red-600 border border-red-200 px-1.5 py-0.2 rounded-full text-[10px] font-bold">{{ discrepancyCount }}</span>
            </button>
        </nav>

        <!-- Error Toast Notification -->
        <div v-if="errorMessage" class="fixed top-4 right-4 bg-red-500 text-white px-4 py-3 rounded shadow-lg max-w-sm z-50">
            <div class="flex justify-between items-start">
                <span>{{ errorMessage }}</span>
                <button @click="errorMessage = ''" class="ml-2 text-lg">&times;</button>
            </div>
        </div>

        <!-- Success Toast Notification -->
        <div v-if="successMessage" class="fixed top-4 right-4 bg-emerald-500 text-white px-4 py-3 rounded shadow-lg max-w-sm z-50">
            <div class="flex justify-between items-start">
                <span>{{ successMessage }}</span>
                <button @click="successMessage = ''" class="ml-2 text-lg">&times;</button>
            </div>
        </div>

        <!-- Workspace Main Layout -->
        <div class="p-5 grid grid-cols-12 gap-5 max-w-[1600px] mx-auto">

            <!-- Left Sidebar Worklist -->
            <aside class="col-span-3 space-y-4">

                <!-- Workflow Selector -->
                <div class="bg-white p-3.5 rounded-md border border-slate-200 shadow-sm space-y-2">
                    <div class="flex items-center space-x-2">
                        <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Verification workflow</span>
                        <span class="bg-slate-100 text-slate-500 text-[9px] px-1.5 py-0.2 rounded border border-slate-200 font-semibold">LAB SETTING</span>
                    </div>
                    <div class="grid grid-cols-2 gap-1 bg-slate-100 p-1 rounded">
                        <button class="bg-[#2B5270] text-white py-1.5 rounded font-bold shadow-sm">Fast Count</button>
                        <button class="text-slate-600 hover:text-slate-900 py-1.5 rounded font-semibold bg-white border border-slate-200">Full Scan</button>
                    </div>
                </div>

                <!-- Search Input -->
                <div class="bg-white p-3.5 rounded-md border border-slate-200 shadow-sm space-y-1.5">
                    <div class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Find Manifest</div>
                    <div class="relative">
                        <input type="text" v-model="searchQuery" placeholder="Scan or search manifest..."
                               class="w-full bg-slate-50/80 border border-slate-300 rounded px-3 py-2 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500 placeholder-slate-400" />
                    </div>
                </div>

                <!-- Verify & Receive Counter Card -->
                <div class="bg-white p-3.5 rounded-md border border-slate-200 shadow-sm space-y-3">
                    <div class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Verify & Receive</div>
                    <div class="text-slate-600 font-medium text-[11px]">Total bottles counted by lab tech</div>

                    <div class="flex items-center space-x-2">
                        <button @click="countedBottles = Math.max(0, countedBottles - 1)"
                                class="w-10 h-9 border border-slate-300 rounded flex items-center justify-center font-bold text-slate-600 hover:bg-slate-50 text-base">
                            -
                        </button>
                        <div class="flex-1 h-9 border border-slate-300 rounded flex items-center justify-center font-bold text-slate-800 text-base bg-white">
                            {{ countedBottles }}
                        </div>
                        <button @click="countedBottles++"
                                class="w-10 h-9 border border-slate-300 rounded flex items-center justify-center font-bold text-slate-600 hover:bg-slate-50 text-base">
                            +
                        </button>
                    </div>

                    <div v-if="activeManifest" class="text-[11px] font-bold" :class="countedBottles === activeManifest.specimens.length ? 'text-emerald-700' : 'text-amber-700'">
                        Matches {{ activeManifest.specimens.length }} expected — {{ countedBottles === activeManifest.specimens.length ? 'ready to close.' : 'mismatch detected.' }}
                    </div>
                </div>

                <!-- Recent Manifests List -->
                <div class="bg-white p-3.5 rounded-md border border-slate-200 shadow-sm space-y-2.5">
                    <div class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Recent Manifests</div>

                    <div v-if="isLoading" class="space-y-2">
                        <div v-for="i in 3" :key="i" class="h-20 bg-slate-100 rounded animate-pulse"></div>
                    </div>

                    <div v-else class="space-y-2">
                        <div v-for="m in filteredManifests" :key="m.id" @click="selectManifest(m.id)"
                             :class="['p-3 rounded border cursor-pointer transition', activeManifest?.id === m.id ? 'border-blue-400 bg-slate-50/80 shadow-sm' : 'border-slate-200 hover:bg-slate-50']">
                            <div class="flex justify-between items-start">
                                <span class="font-bold text-slate-900 text-xs">{{ m.code }}</span>
                                <span class="text-[11px] text-slate-500 font-medium">{{ getReceivedCount(m) }}/{{ m.specimens.length }} received</span>
                            </div>
                            <div class="text-[11px] text-slate-500 mt-0.5">{{ m.originClinic }}</div>
                            <div class="mt-2 flex justify-start">
                                <span :class="getBadgeStyles(m.status)" class="text-[10px] px-2 py-0.5 rounded font-bold uppercase tracking-wide">
                                    {{ formatStatusText(m.status) }}
                                </span>
                            </div>
                        </div>
                    </div>

                    <button class="w-full text-center text-slate-600 hover:text-slate-900 text-[11px] font-bold pt-2 border-t border-slate-100 flex items-center justify-center space-x-1">
                        <span>View all manifests</span>
                        <span>›</span>
                    </button>
                </div>

            </aside>

            <!-- Right Detail Workspace Panel -->
            <main class="col-span-9 space-y-4" v-if="activeManifest">

                <!-- Header Banner -->
                <div class="bg-white p-5 rounded-md border border-slate-200 shadow-sm flex justify-between items-start">
                    <div class="space-y-1">
                        <div class="flex items-center space-x-2">
                            <h1 class="text-lg font-bold text-slate-900">Manifest {{ activeManifest.code }}</h1>
                            <span class="bg-slate-100 text-slate-600 border border-slate-200 text-[10px] px-2 py-0.5 rounded font-bold uppercase">Fast Count</span>
                        </div>
                        <p class="text-slate-500 text-xs">
                            From <span class="font-semibold text-slate-700">{{ activeManifest.originClinic }}</span> — Sent {{ formatDate(activeManifest.sentAt) }} ·
                            <span class="font-bold text-slate-800">{{ activeManifest.specimens.length }} specimens expected</span> · Khush Arya
                        </p>
                    </div>

                    <div class="flex items-center space-x-2">
                        <button class="px-3 py-1.5 border border-red-300 text-red-600 hover:bg-red-50 rounded font-bold text-xs flex items-center space-x-1">
                            <span>🚩</span>
                            <span>Flag discrepancy</span>
                        </button>
                        <button @click="closeManifest"
                                :disabled="pendingCount > 0 || isProcessing"
                                :class="['px-4 py-1.5 rounded font-bold text-xs text-white shadow-sm transition', pendingCount === 0 && !isProcessing ? 'bg-[#2B5270] hover:bg-[#1C3644] cursor-pointer' : 'bg-slate-300 cursor-not-allowed']">
                            {{ isProcessing ? 'Closing...' : 'Mark Received & Close' }}
                        </button>
                    </div>
                </div>

                <!-- Metric KPI Cards -->
                <div class="grid grid-cols-4 gap-4">
                    <div class="bg-white p-4 rounded-md border border-slate-200 text-center shadow-sm">
                        <div class="text-2xl font-black text-slate-800">{{ activeManifest.specimens.length }}</div>
                        <div class="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-1">EXPECTED</div>
                    </div>
                    <div class="bg-white p-4 rounded-md border border-slate-200 text-center shadow-sm">
                        <div class="text-2xl font-black text-emerald-600">{{ receivedCount }}</div>
                        <div class="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-1">RECEIVED</div>
                    </div>
                    <div class="bg-white p-4 rounded-md border border-slate-200 text-center shadow-sm">
                        <div class="text-2xl font-black text-amber-500">{{ pendingCount }}</div>
                        <div class="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-1">PENDING</div>
                    </div>
                    <div class="bg-white p-4 rounded-md border border-slate-200 text-center shadow-sm">
                        <div class="text-2xl font-black text-red-500">{{ flaggedCount }}</div>
                        <div class="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-1">FLAGGED</div>
                    </div>
                </div>

                <!-- Table View -->
                <div class="bg-white rounded-md border border-slate-200 shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-slate-200 flex justify-between items-center bg-slate-50/50">
                        <h2 class="text-xs font-bold text-slate-700">Specimens on manifest</h2>
                        <span class="text-[11px] bg-emerald-100 text-emerald-800 border border-emerald-200 px-2.5 py-0.5 rounded font-bold">
                            {{ receivedCount }} received
                        </span>
                    </div>

                    <table class="w-full text-left text-xs border-collapse">
                        <thead>
                            <tr class="bg-slate-50 text-slate-500 uppercase tracking-wider font-bold border-b border-slate-200 text-[10px]">
                                <th class="px-5 py-2.5">STATUS</th>
                                <th class="px-5 py-2.5">SPECIMEN ID</th>
                                <th class="px-5 py-2.5">PATIENT</th>
                                <th class="px-5 py-2.5">SITE</th>
                                <th class="px-5 py-2.5">PROVIDER</th>
                                <th class="px-5 py-2.5">RECEIVED BY</th>
                                <th class="px-5 py-2.5">AT</th>
                                <th class="px-5 py-2.5 text-right">ACTIONS</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-slate-100">
                            <tr v-for="s in activeManifest.specimens" :key="s.id" class="hover:bg-slate-50/80 transition">
                                <td class="px-5 py-2.5">
                                    <span v-if="s.status === 1" class="bg-emerald-100 text-emerald-800 border border-emerald-200 px-2 py-0.5 rounded font-bold text-[10px] inline-flex items-center space-x-1">
                                        <span>✓</span> <span>Received</span>
                                    </span>
                                    <span v-else-if="s.status === 2" class="bg-red-100 text-red-800 border border-red-200 px-2 py-0.5 rounded font-bold text-[10px] inline-flex items-center space-x-1">
                                        <span>🚩</span> <span>Flagged</span>
                                    </span>
                                    <span v-else class="bg-slate-100 text-slate-600 border border-slate-200 px-2 py-0.5 rounded font-bold text-[10px]">
                                        Pending
                                    </span>
                                </td>
                                <td class="px-5 py-2.5 font-bold text-slate-900">{{ s.code }}</td>
                                <td class="px-5 py-2.5 text-slate-700 font-medium">{{ s.patientName }}</td>
                                <td class="px-5 py-2.5 text-slate-600">{{ s.site }}</td>
                                <td class="px-5 py-2.5 text-slate-600">{{ s.provider }}</td>
                                <td class="px-5 py-2.5 text-slate-600">{{ s.receivedBy || '—' }}</td>
                                <td class="px-5 py-2.5 text-slate-600">{{ s.receivedAt ? formatTime(s.receivedAt) : '—' }}</td>
                                <td class="px-5 py-2.5 text-right space-x-1">
                                    <button @click="markReceived(s.id)" :disabled="isProcessing" class="px-2 py-1 border border-slate-200 rounded text-slate-600 hover:bg-slate-100 hover:text-slate-900 font-bold disabled:opacity-50">✏️</button>
                                    <button @click="flagMissing(s.id)" :disabled="isProcessing" class="px-2 py-1 border border-slate-200 rounded text-slate-600 hover:bg-slate-100 hover:text-slate-900 font-bold disabled:opacity-50">🚩</button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>

            </main>

            <!-- Empty State -->
            <main v-else class="col-span-9 flex items-center justify-center">
                <div class="text-center text-slate-500">
                    <p class="text-lg font-semibold mb-2">No manifest selected</p>
                    <p class="text-sm">Select a manifest from the list to get started</p>
                </div>
            </main>
        </div>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted } from 'vue';
    import api from './api';

    const manifests = ref([]);
    const activeManifest = ref(null);
    const countedBottles = ref(0);
    const searchQuery = ref('');
    const isLoading = ref(false);
    const isProcessing = ref(false);
    const errorMessage = ref('');
    const successMessage = ref('');

    const fetchManifests = async () => {
        isLoading.value = true;
        try {
            const res = await api.get('/manifests');
            manifests.value = res.data;
            if (manifests.value.length > 0) {
                await selectManifest(manifests.value[0].id);
            }
        } catch (err) {
            errorMessage.value = "Failed to load manifests. Please try again.";
            console.error("API Error:", err);
        } finally {
            isLoading.value = false;
        }
    };

    const selectManifest = async (id) => {
        try {
            const res = await api.get(`/manifests/${id}`);
            activeManifest.value = res.data;
            countedBottles.value = activeManifest.value.specimens.filter(s => s.status === 1).length;
        } catch (err) {
            errorMessage.value = "Failed to load manifest details.";
            console.error("Error fetching manifest detail:", err);
        }
    };

    const markReceived = async (specimenId) => {
        isProcessing.value = true;
        try {
            await api.post(`/manifests/${activeManifest.value.id}/specimens/${specimenId}/receive`);
            await selectManifest(activeManifest.value.id);
            successMessage.value = "Specimen marked as received.";
            setTimeout(() => successMessage.value = '', 3000);
        } catch (err) {
            errorMessage.value = err.response?.data?.error || "Failed to mark specimen received.";
            console.error("Error marking specimen received:", err);
        } finally {
            isProcessing.value = false;
        }
    };

    const flagMissing = async (specimenId) => {
        isProcessing.value = true;
        try {
            await api.post(`/manifests/${activeManifest.value.id}/specimens/${specimenId}/flag`);
            await selectManifest(activeManifest.value.id);
            successMessage.value = "Specimen flagged as missing.";
            setTimeout(() => successMessage.value = '', 3000);
        } catch (err) {
            errorMessage.value = err.response?.data?.error || "Failed to flag specimen.";
            console.error("Error flagging specimen:", err);
        } finally {
            isProcessing.value = false;
        }
    };

    const closeManifest = async () => {
        if (pendingCount.value > 0) {
            errorMessage.value = `Cannot close manifest with ${pendingCount.value} pending specimens.`;
            return;
        }

        isProcessing.value = true;
        try {
            await api.post(`/manifests/${activeManifest.value.id}/close`);
            successMessage.value = "Manifest closed successfully.";
            await fetchManifests();
            setTimeout(() => successMessage.value = '', 3000);
        } catch (err) {
            errorMessage.value = err.response?.data?.error || "Failed to close manifest.";
            console.error("Error closing manifest:", err);
        } finally {
            isProcessing.value = false;
        }
    };

    const receivedCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 1).length || 0);
    const flaggedCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 2).length || 0);
    const pendingCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 0).length || 0);
    const discrepancyCount = computed(() => flaggedCount.value);

    const filteredManifests = computed(() => {
        if (!searchQuery.value) return manifests.value;
        const query = searchQuery.value.toLowerCase();
        return manifests.value.filter(m =>
            m.code.toLowerCase().includes(query) ||
            m.originClinic.toLowerCase().includes(query)
        );
    });

    const getReceivedCount = (manifest) => manifest.specimens.filter(s => s.status === 1).length;

    const formatTime = (dt) => {
        if (!dt) return '—';
        const date = new Date(dt);
        return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
    };

    const formatDate = (dt) => {
        if (!dt) return '—';
        const date = new Date(dt);
        return date.toLocaleDateString([], { month: 'short', day: 'numeric', year: 'numeric' }) + ', ' +
            date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
    };

    const formatStatusText = (status) => {
        if (status === 0) return 'In transit';
        if (status === 1) return 'Received';
        return '1 discrepancy';
    };

    const getBadgeStyles = (status) => {
        if (status === 0) return 'bg-slate-100 text-slate-600 border border-slate-200';
        if (status === 1) return 'bg-emerald-100 text-emerald-800 border border-emerald-200';
        return 'bg-red-100 text-red-700 border border-red-200';
    };

    onMounted(fetchManifests);
</script>