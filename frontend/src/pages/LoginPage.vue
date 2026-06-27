<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function handleSubmit() {
  error.value = ''
  loading.value = true
  try {
    await authStore.login(email.value, password.value)
    await router.push('/dashboard')
  } catch (err: any) {
    error.value = err?.response?.data?.message ?? 'Invalid email or password. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4" style="background-color: #eef2f7;">
    <div class="w-full max-w-md">
      <div class="bg-white rounded-2xl shadow-lg p-10">
        <!-- Logo -->
        <div class="flex flex-col items-center mb-8">
          <div class="w-16 h-16 rounded-full border-2 flex items-center justify-center mb-4" style="border-color: #38bdf8;">
            <span class="text-3xl">⚡</span>
          </div>
          <h1 class="text-3xl font-extrabold" style="color: #1e3a8a;">Welcome Back</h1>
          <p class="mt-1 text-sm text-gray-500">Log in to your PlayLeague account</p>
        </div>

        <!-- Error Banner -->
        <div
          v-if="error"
          class="mb-5 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700"
        >
          {{ error }}
        </div>

        <!-- Form -->
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <input
            v-model="email"
            type="email"
            required
            autocomplete="email"
            placeholder="Email Address *"
            class="w-full px-4 py-3 border border-gray-200 rounded-xl text-sm text-gray-700 placeholder-gray-400 focus:outline-none focus:ring-2 focus:border-transparent transition"
            style="focus-ring-color: #3b82f6;"
          />

          <input
            v-model="password"
            type="password"
            required
            autocomplete="current-password"
            placeholder="Password *"
            class="w-full px-4 py-3 border border-gray-200 rounded-xl text-sm text-gray-700 placeholder-gray-400 focus:outline-none focus:ring-2 focus:border-transparent transition"
          />

          <button
            type="submit"
            :disabled="loading || !email || !password"
            class="w-full font-semibold py-3 px-4 rounded-xl text-sm transition-colors focus:outline-none"
            :class="loading || !email || !password
              ? 'bg-gray-200 text-gray-400 cursor-not-allowed'
              : 'bg-blue-600 hover:bg-blue-700 text-white'"
          >
            <span v-if="loading">Signing in…</span>
            <span v-else>Log In</span>
          </button>
        </form>

        <!-- Footer -->
        <p class="mt-6 text-center text-sm text-gray-500">
          Don't have an account?
          <RouterLink to="/register" class="font-semibold" style="color: #1e3a8a;">
            Sign up free
          </RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>
