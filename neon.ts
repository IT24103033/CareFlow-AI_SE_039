import { defineConfig } from "@neon/config/v1";

export default defineConfig({
  preview: {
    buckets: {
      "triage-images": { access: "public_read" }
    }
  }
});
