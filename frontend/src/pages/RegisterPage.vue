<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()

const name = ref('')
const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)
const success = ref(false)

async function handleSubmit() {
  error.value = ''
  loading.value = true
  try {
    await authStore.register(email.value, password.value, name.value)
    success.value = true
  } catch (err: any) {
    error.value = err?.response?.data?.message ?? 'Registration failed. Please try again.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center p-4" style="background-color: #eef2f7;">
    <div class="w-full max-w-md">
      <div class="bg-white rounded-2xl shadow-lg p-10">
        <!-- Header -->
        <div class="flex flex-col items-center mb-8">
          <div class="w-16 h-16 rounded-full border-2 flex items-center justify-center mb-4" style="border-color: #38bdf8;">
            <span class="text-3xl">⚡</span>
          </div>
          <h1 class="text-3xl font-extrabold" style="color: #1e3a8a;">Create Account</h1>
          <p class="mt-1 text-sm text-gray-500">Join PlayLeague today</p>
        </div>

        <!-- Success Message -->
        <div
          v-if="success"
          class="p-4 bg-green-50 border border-green-200 rounded-lg text-sm text-green-700"
        >
          <p class="font-semibold mb-1">Registration submitted!</p>
          <p>Your account is pending approval by a league administrator. You'll receive an email once your account is activated.</p>
        </div>

        <!-- Form (hidden after success) -->
        <template v-else>
          <!-- Error Banner -->
          <div
            v-if="error"
            class="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-sm text-red-700"
          >
            {{ error }}
          </div>

          <form @submit.prevent="handleSubmit" class="space-y-4">
            <div>
              <label for="name" class="block text-sm font-medium text-gray-700 mb-1">
                Full name
              </label>
              <input
                id="name"
                v-model="name"
                type="text"
                required
                autocomplete="name"
                placeholder="Jane Smith"
                class="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              />
            </div>

            <div>
              <label for="email" class="block text-sm font-medium text-gray-700 mb-1">
                Email address
              </label>
              <input
                id="email"
                v-model="email"
                type="email"
                required
                autocomplete="email"
                placeholder="you@example.com"
                class="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              />
            </div>

            <div>
              <label for="password" class="block text-sm font-medium text-gray-700 mb-1">
                Password
              </label>
              <input
                id="password"
                v-model="password"
                type="password"
                required
                minlength="8"
                autocomplete="new-password"
                placeholder="Minimum 8 characters"
                class="w-full px-3 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition"
              />
            </div>

            <button
              type="submit"
              :disabled="loading"
              class="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-semibold py-2.5 px-4 rounded-lg text-sm transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
            >
              <span v-if="loading">Creating account…</span>
              <span v-else>Create account</span>
            </button>
          </form>
        </template>

        <!-- Footer -->
        <p class="mt-6 text-center text-sm text-gray-500">
          Already have an account?
          <RouterLink to="/login" class="font-medium text-blue-600 hover:text-blue-500">
            Sign in
          </RouterLink>
        </p>
      </div>
    </div>
  </div>
</template>
