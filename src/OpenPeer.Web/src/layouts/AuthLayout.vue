<template>
  <div class="auth-layout">
    <div class="auth-card">
      <h1>OpenPeer</h1>
      <router-view />
    </div>
  </div>
</template>

<style scoped>
.auth-layout {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: #f5f7fa;
}
.auth-card {
  width: 400px;
  padding: 40px;
  background: #fff;
  border-radius: 8px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}
.auth-card h1 {
  text-align: center;
  margin-bottom: 32px;
  color: #409eff;
}
</style>
