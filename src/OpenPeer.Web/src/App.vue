<template>
  <DefaultLayout v-if="$route.meta.layout !== 'auth'" />
  <AuthLayout v-else>
    <router-view />
  </AuthLayout>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useAuthStore } from "./stores/auth";
import DefaultLayout from "./layouts/DefaultLayout.vue";
import AuthLayout from "./layouts/AuthLayout.vue";

const auth = useAuthStore();
onMounted(() => auth.initialize());
</script>

<style>
canvas.hiddenCanvasElement {
  position: absolute !important;
  top: 0 !important;
  left: 0 !important;
  width: 0 !important;
  height: 0 !important;
  display: none !important;
}
</style>
