<template>
  <DefaultLayout v-if="$route.meta.layout !== 'auth'" />
  <AuthLayout v-else>
    <router-view />
  </AuthLayout>
</template>

<script setup lang="ts">
import DefaultLayout from "./layouts/DefaultLayout.vue";
import AuthLayout from "./layouts/AuthLayout.vue";
</script>
