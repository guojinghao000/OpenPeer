import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/auth";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      name: "home",
      component: () => import("../views/HomeView.vue"),
    },
    {
      path: "/login",
      name: "login",
      component: () => import("../views/LoginView.vue"),
      meta: { guest: true, layout: "auth" },
    },
    {
      path: "/register",
      name: "register",
      component: () => import("../views/RegisterView.vue"),
      meta: { guest: true, layout: "auth" },
    },
    {
      path: "/upload",
      name: "upload",
      component: () => import("../views/PaperUploadView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: "/papers/:id",
      name: "paperDetail",
      component: () => import("../views/PaperDetailView.vue"),
    },
    {
      path: "/papers/:id/edit",
      name: "paperEdit",
      component: () => import("../views/PaperEditView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: "/admin",
      name: "admin",
      component: () => import("../views/admin/AdminLayout.vue"),
      meta: { requiresAuth: true, requiresAdmin: true },
      children: [
        {
          path: "categories",
          name: "adminCategories",
          component: () => import("../views/admin/CategoryManageView.vue"),
        },
        {
          path: "users",
          name: "adminUsers",
          component: () => import("../views/admin/UserManageView.vue"),
        },
      ],
    },
    {
      path: "/profile",
      name: "profile",
      component: () => import("../views/ProfileView.vue"),
      meta: { requiresAuth: true },
    },
    {
      path: "/:pathMatch(.*)*",
      name: "notFound",
      component: () => import("../views/NotFoundView.vue"),
    },
  ],
});

router.beforeEach((to, _from, next) => {
  const auth = useAuthStore();
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    next("/login");
  } else if (to.meta.requiresAdmin && auth.user?.role !== "Admin") {
    next("/");
  } else if (to.meta.guest && auth.isAuthenticated) {
    next("/");
  } else {
    next();
  }
});

export default router;
