<template>
  <div class="pdf-viewer-wrapper">
    <div v-if="loading" class="pdf-loading">
      <el-skeleton :rows="6" animated />
    </div>

    <div v-if="error" class="pdf-error">
      <el-result
        icon="error"
        title="PDF 加载失败"
        sub-title="文件可能已损坏或网络异常，请尝试下载后查看"
      >
        <template #extra>
          <a :href="src" target="_blank">
            <el-button type="primary">下载 PDF</el-button>
          </a>
        </template>
      </el-result>
    </div>

    <div v-show="!loading && !error" class="pdf-viewer-content">
      <div class="pdf-toolbar">
        <div class="toolbar-group">
          <el-button-group>
            <el-button
              size="small"
              @click="zoomOut"
              :disabled="scaleIndex <= 0"
            >
              <el-icon><ZoomOut /></el-icon>
            </el-button>
            <el-button size="small" disabled class="zoom-label">
              {{ Math.round(scale * 100) }}%
            </el-button>
            <el-button
              size="small"
              @click="zoomIn"
              :disabled="scaleIndex >= zoomLevels.length - 1"
            >
              <el-icon><ZoomIn /></el-icon>
            </el-button>
          </el-button-group>
        </div>

        <div class="toolbar-group">
          <el-button-group>
            <el-button
              size="small"
              @click="prevPage"
              :disabled="currentPage <= 1"
            >
              <el-icon><ArrowLeft /></el-icon>
            </el-button>
            <el-button size="small" disabled class="page-label">
              <span class="page-input-wrapper">
                <el-input-number
                  v-model="currentPage"
                  :min="1"
                  :max="totalPages"
                  size="small"
                  :controls="false"
                  class="page-input"
                />
                <span class="page-total">/ {{ totalPages }}</span>
              </span>
            </el-button>
            <el-button
              size="small"
              @click="nextPage"
              :disabled="currentPage >= totalPages"
            >
              <el-icon><ArrowRight /></el-icon>
            </el-button>
          </el-button-group>
        </div>

        <div class="toolbar-group">
          <el-button size="small" @click="handlePrint">
            <el-icon><Printer /></el-icon>
            打印
          </el-button>
          <a :href="src" target="_blank">
            <el-button size="small" type="primary">
              <el-icon><Download /></el-icon>
              下载
            </el-button>
          </a>
        </div>
      </div>

      <div class="pdf-container">
        <VuePdfEmbed
          ref="pdfRef"
          :source="src"
          :page="currentPage"
          :scale="scale"
          :text-layer="true"
          :annotation-layer="true"
          @loaded="onLoaded"
          @loading-failed="onError"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, shallowRef } from "vue";
import VuePdfEmbed from "vue-pdf-embed";
import {
  ZoomIn,
  ZoomOut,
  ArrowLeft,
  ArrowRight,
  Printer,
  Download,
} from "@element-plus/icons-vue";
import { ElMessage } from "element-plus";
import type { PDFDocumentProxy } from "pdfjs-dist/types/src/display/api";

const props = defineProps<{
  src: string;
}>();

const pdfRef = ref<InstanceType<typeof VuePdfEmbed>>();
const loading = ref(true);
const error = ref(false);
const doc = shallowRef<PDFDocumentProxy | null>(null);
const currentPage = ref(1);
const totalPages = ref(0);
const scaleIndex = ref(2);

const zoomLevels = [0.25, 0.5, 0.75, 1.0, 1.25, 1.5, 2.0, 3.0, 4.0];
const scale = computed(() => zoomLevels[scaleIndex.value]);
console.log(props.src);
function onLoaded(pdfDoc: PDFDocumentProxy) {
  doc.value = pdfDoc;
  totalPages.value = pdfDoc.numPages;
  currentPage.value = 1;
  loading.value = false;
}

function onError(e: unknown) {
  console.error("PDF loading failed:", e);
  loading.value = false;
  error.value = true;
}

function prevPage() {
  if (currentPage.value > 1) currentPage.value--;
}

function nextPage() {
  if (currentPage.value < totalPages.value) currentPage.value++;
}

function zoomIn() {
  if (scaleIndex.value < zoomLevels.length - 1) scaleIndex.value++;
}

function zoomOut() {
  if (scaleIndex.value > 0) scaleIndex.value--;
}

async function handlePrint() {
  try {
    await pdfRef.value?.print(150, undefined, true);
  } catch {
    ElMessage.error("打印失败");
  }
}
</script>

<style scoped>
.pdf-viewer-wrapper {
  margin: 16px 0;
}

.pdf-loading {
  padding: 40px;
  border: 1px solid var(--el-border-color-light, #ebeef5);
  border-radius: 4px;
}

.pdf-error {
  border: 1px solid var(--el-border-color-light, #ebeef5);
  border-radius: 4px;
  padding: 20px;
}

.pdf-toolbar {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 12px;
  padding: 8px 12px;
  border: 1px solid var(--el-border-color-light, #ebeef5);
  border-radius: 4px;
  background: var(--el-fill-color-light, #f5f7fa);
}

.toolbar-group {
  display: flex;
  align-items: center;
  gap: 4px;
}

.zoom-label {
  min-width: 52px;
  font-variant-numeric: tabular-nums;
}

.page-label {
  min-width: 100px;
}

.page-input-wrapper {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.page-input {
  width: 56px;
}

.page-input :deep(.el-input__inner) {
  text-align: center;
  padding: 0 4px;
}

.page-total {
  white-space: nowrap;
  color: var(--el-text-color-secondary, #909399);
  font-size: 13px;
}

.pdf-container {
  border: 1px solid var(--el-border-color-light, #ebeef5);
  border-radius: 4px;
  overflow-y: auto;
  overflow-x: hidden;
  max-height: 80vh;
  line-height: 0;
  font-size: 0;
}

.pdf-container :deep(.vue-pdf-embed) {
  line-height: 0;
  font-size: 0;
}

.pdf-container :deep(.vue-pdf-embed > div) {
  margin: 0;
  padding: 0;
  line-height: 0;
  font-size: 0;
}

.pdf-container :deep(.vue-pdf-embed__page) {
  margin: 0;
  padding: 0;
  line-height: 0;
  font-size: 0;
}

.pdf-container :deep(canvas) {
  display: block;
}

.pdf-container :deep(.textLayer),
.pdf-container :deep(.annotationLayer) {
  position: absolute !important;
  top: 0 !important;
  left: 0 !important;
  pointer-events: none;
}
</style>
