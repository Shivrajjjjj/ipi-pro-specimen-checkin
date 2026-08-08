<!--<script setup>
    import HelloWorld from './components/HelloWorld.vue'
</script>-->

<template>
    <div class="min-h-screen bg-[#F4F6F8] font-sans text-gray-800">
        <header class="bg-[#0A2540] text-white px-6 py-3 flex items-center justify-between shadow-md">
            <div class="flex items-center space-x-6">
                <div class="bg-white text-[#0A2540] font-extrabold px-2 py-1 rounded text-lg tracking-wider">IPI</div>
                <span class="bg-blue-900 text-xs px-2 py-0.5 rounded font-semibold text-blue-200">UAT</span>
                <div class="text-sm font-medium">
                    <span class="text-gray-400">Mode:</span> Check-In
                    <span class="text-gray-400 ml-4">Location:</span> <span class="font-bold">Central Lab — Receiving</span>
                </div>
            </div>
            <div class="flex items-center space-x-3 text-sm">
                <span>Lab Tech 1</span>
                <div class="w-8 h-8 rounded-full bg-blue-700 text-white flex items-center justify-center font-bold text-xs">LT</div>
            </div>
        </header>

        <nav class="bg-white border-b px-6 py-2 flex space-x-8 text-sm font-semibold">
            <button class="text-[#1E56A0] border-b-2 border-[#1E56A0] pb-2">Check-In</button>
            <button class="text-gray-500 hover:text-gray-800 pb-2">Scan History <span class="bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full text-xs">12</span></button>
            <button class="text-gray-500 hover:text-gray-800 pb-2">Manifests</button>
            <button class="text-gray-500 hover:text-gray-800 pb-2">Discrepancies <span class="bg-red-100 text-red-600 px-2 py-0.5 rounded-full text-xs">5</span></button>
        </nav>

        <div class="p-6 grid grid-cols-12 gap-6">
            <aside class="col-span-3 space-y-4">
                <div class="bg-white p-4 rounded-lg border border-gray-200 shadow-sm">
                    <div class="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">Verification Workflow</div>
                    <div class="grid grid-cols-2 gap-2 bg-gray-100 p-1 rounded-md">
                        <button class="bg-[#1E56A0] text-white py-1.5 rounded text-sm font-medium">Fast Count</button>
                        <button class="text-gray-600 py-1.5 rounded text-sm font-medium">Full Scan</button>
                    </div>
                </div>

                <div class="bg-white p-4 rounded-lg border border-gray-200 shadow-sm space-y-2">
                    <div class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Find Manifest</div>
                    <input type="text" placeholder="Scan or search manifest..." class="w-full bg-gray-50 border border-gray-300 rounded px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-blue-500" />
                </div>

                <div class="bg-white p-4 rounded-lg border border-gray-200 shadow-sm space-y-3">
                    <div class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Recent Manifests</div>
                    <div v-for="m in manifests" :key="m.id" @click="selectManifest(m.id)"
                         :class="['p-3 rounded-lg border cursor-pointer transition', activeManifest?.id === m.id ? 'border-blue-500 bg-blue-50/30' : 'border-gray-200 hover:bg-gray-50']">
                        <div class="flex justify-between items-start">
                            <span class="font-bold text-gray-800">{{ m.code }}</span>
                            <span class="text-xs text-gray-500">{{ getReceivedCount(m) }}/{{ m.specimens.length }} received</span>
                        </div>
                        <div class="text-xs text-gray-500 mt-1">{{ m.originClinic }}</div>
                        <div class="mt-2">
                            <span :class="getStatusBadgeClass(m.status)" class="text-[10px] px-2 py-0.5 rounded-full font-semibold uppercase">
                                {{ m.status }}
                            </span>
                        </div>
                    </div>
                </div>
            </aside>

            <main class="col-span-9 space-y-4" v-if="activeManifest">
                <div class="bg-white p-6 rounded-lg border border-gray-200 shadow-sm flex justify-between items-center">
                    <div>
                        <div class="flex items-center space-x-3">
                            <h1 class="text-xl font-bold text-gray-900">Manifest {{ activeManifest.code }}</h1>
                            <span class="bg-blue-100 text-blue-800 text-xs px-2 py-0.5 rounded font-medium">Fast Count</span>
                        </div>
                        <p class="text-xs text-gray-500 mt-1">
                            From <span class="font-medium text-gray-700">{{ activeManifest.originClinic }}</span> — Sent {{ formatDate(activeManifest.sentAt) }}
                        </p>
                    </div>
                    <button @click="closeManifest" :disabled="pendingCount > 0"
                            :class="['px-4 py-2 rounded text-sm font-semibold text-white shadow-sm transition', pendingCount === 0 ? 'bg-[#1E56A0] hover:bg-blue-800' : 'bg-gray-300 cursor-not-allowed']">
                        Mark Received & Close
                    </button>
                </div>

                <div class="grid grid-cols-4 gap-4">
                    <div class="bg-white p-4 rounded-lg border border-gray-200 text-center">
                        <div class="text-2xl font-bold text-gray-800">{{ activeManifest.specimens.length }}</div>
                        <div class="text-xs font-semibold text-gray-400 tracking-wider uppercase mt-1">Expected</div>
                    </div>
                    <div class="bg-white p-4 rounded-lg border border-gray-200 text-center">
                        <div class="text-2xl font-bold text-green-600">{{ receivedCount }}</div>
                        <div class="text-xs font-semibold text-gray-400 tracking-wider uppercase mt-1">Received</div>
                    </div>
                    <div class="bg-white p-4 rounded-lg border border-gray-200 text-center">
                        <div class="text-2xl font-bold text-yellow-600">{{ pendingCount }}</div>
                        <div class="text-xs font-semibold text-gray-400 tracking-wider uppercase mt-1">Pending</div>
                    </div>
                    <div class="bg-white p-4 rounded-lg border border-gray-200 text-center">
                        <div class="text-2xl font-bold text-red-600">{{ flaggedCount }}</div>
                        <div class="text-xs font-semibold text-gray-400 tracking-wider uppercase mt-1">Flagged</div>
                    </div>
                </div>

                <div class="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
                    <div class="px-6 py-4 border-b flex justify-between items-center bg-gray-50">
                        <h2 class="text-sm font-bold text-gray-700">Specimens on manifest</h2>
                        <span class="text-xs bg-green-100 text-green-800 px-2.5 py-1 rounded-full font-medium">{{ receivedCount }} received</span>
                    </div>

                    <table class="w-full text-left text-xs">
                        <thead class="bg-gray-100 text-gray-500 uppercase tracking-wider font-semibold border-b">
                            <tr>
                                <th class="px-6 py-3">Status</th>
                                <th class="px-6 py-3">Specimen ID</th>
                                <th class="px-6 py-3">Patient</th>
                                <th class="px-6 py-3">Site</th>
                                <th class="px-6 py-3">Provider</th>
                                <th class="px-6 py-3">Actions</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-gray-100">
                            <tr v-for="s in activeManifest.specimens" :key="s.id" class="hover:bg-gray-50">
                                <td class="px-6 py-3">
                                    <span v-if="s.status === 1" class="bg-green-100 text-green-700 px-2 py-0.5 rounded-md font-semibold">✓ Received</span>
                                    <span v-else-if="s.status === 2" class="bg-red-100 text-red-700 px-2 py-0.5 rounded-md font-semibold">🚩 Flagged</span>
                                    <span v-else class="bg-gray-100 text-gray-600 px-2 py-0.5 rounded-md font-semibold">Pending</span>
                                </td>
                                <td class="px-6 py-3 font-semibold text-gray-900">{{ s.code }}</td>
                                <td class="px-6 py-3 font-medium">{{ s.patientName }}</td>
                                <td class="px-6 py-3 text-gray-500">{{ s.site }}</td>
                                <td class="px-6 py-3 text-gray-500">{{ s.provider }}</td>
                                <td class="px-6 py-3 space-x-2">
                                    <button @click="markReceived(s.id)" class="px-2 py-1 bg-green-50 text-green-700 border border-green-200 rounded hover:bg-green-100 font-medium">Receive</button>
                                    <button @click="flagMissing(s.id)" class="px-2 py-1 bg-red-50 text-red-700 border border-red-200 rounded hover:bg-red-100 font-medium">Flag</button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
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

    const fetchManifests = async () => {
        const res = await api.get('/manifests');
        manifests.value = res.data;
        if (manifests.value.length > 0) {
            selectManifest(manifests.value[0].id);
        }
    };

    const selectManifest = async (id) => {
        const res = await api.get(`/manifests/${id}`);
        activeManifest.value = res.data;
    };

    const markReceived = async (specimenId) => {
        await api.post(`/manifests/${activeManifest.value.id}/specimens/${specimenId}/receive`);
        await selectManifest(activeManifest.value.id);
    };

    const flagMissing = async (specimenId) => {
        await api.post(`/manifests/${activeManifest.value.id}/specimens/${specimenId}/flag`);
        await selectManifest(activeManifest.value.id);
    };

    const closeManifest = async () => {
        await api.post(`/manifests/${activeManifest.value.id}/close`);
        await fetchManifests();
    };

    const receivedCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 1).length || 0);
    const flaggedCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 2).length || 0);
    const pendingCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 0).length || 0);

    const getReceivedCount = (manifest) => manifest.specimens.filter(s => s.status === 1).length;
    const formatDate = (dt) => new Date(dt).toLocaleString();
    const getStatusBadgeClass = (status) => status === 0 ? 'bg-blue-100 text-blue-800' : 'bg-gray-100 text-gray-800';

    onMounted(fetchManifests);
</script>
