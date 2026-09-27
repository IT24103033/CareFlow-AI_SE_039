export function readPlan(raw) {
  try {
    const plan = JSON.parse(raw);
    if (!plan || typeof plan !== 'object' || Array.isArray(plan)) return null;
    const normalized = Object.fromEntries(Object.entries(plan).map(([key, value]) => [key.toLowerCase(), value]));
    const required = ['suggestedspecialist', 'urgencylevel', 'recommendedaction', 'rationale'];
    const validAssessment = required.every(key => typeof normalized[key] === 'string' && normalized[key].trim()) &&
      ['Low', 'Medium', 'High', 'Critical'].includes(normalized.urgencylevel);
    if (!validAssessment) return null;
    const version = normalized.schemaversion ?? 1;
    if (version === 1) return normalized;
    if (version !== 2 || !Array.isArray(normalized.steps) || normalized.steps.length === 0 ||
        !Array.isArray(normalized.warnings) || !normalized.warnings.every(value => typeof value === 'string')) return null;
    const execution = normalize(normalized.execution);
    if (execution.status !== 'Completed' || execution.failurecode) return null;
    normalized.steps = normalized.steps.map(normalize);
    if (!normalized.steps.every(step => ['id', 'agent', 'tool'].every(key => typeof step[key] === 'string' && step[key]) &&
      Array.isArray(step.dependson) && step.dependson.every(value => typeof value === 'string') &&
      typeof step.requireshumanapproval === 'boolean' && step.status === 'Pending')) return null;
    return normalized;
  } catch { return null; }
}

function normalize(value) {
  return value && typeof value === 'object' && !Array.isArray(value)
    ? Object.fromEntries(Object.entries(value).map(([key, field]) => [key.toLowerCase(), field])) : {};
}
