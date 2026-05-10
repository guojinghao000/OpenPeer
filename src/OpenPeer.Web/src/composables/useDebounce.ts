import { ref, watch } from "vue";

export function useDebounce<T>(value: T, delay = 300) {
  const debounced = ref(value) as ReturnType<typeof ref<T>>;

  watch(
    () => value,
    (newVal) => {
      const timer = setTimeout(() => {
        debounced.value = newVal;
      }, delay);
      return () => clearTimeout(timer);
    },
  );

  return debounced;
}
