export const statusLabels = {
  Pending: 'Pending', InReview: 'Awaiting review', Approved: 'Approved',
  Rejected: 'Rejected', RevisionRequested: 'Revision requested',
  AssessmentFailed: 'Assessment unavailable', ReassessmentInProgress: 'Reassessing',
};
export const severityLabels = ['Critical', 'High', 'Medium', 'Low', 'Unassessed'];

export function summarizeDashboard(data) {
  if (!Array.isArray(data?.groups) || !Array.isArray(data?.trend) || !Number.isFinite(Date.parse(data.generatedAt))) {
    throw new Error('The dashboard returned an invalid response. Please refresh.');
  }
  const statuses = Object.fromEntries(Object.keys(statusLabels).map(key => [key, 0]));
  const severities = Object.fromEntries(severityLabels.map(key => [key, 0]));
  let total = 0, urgentReview = 0;
  for (const group of data.groups) {
    if (!Number.isInteger(group.count) || group.count < 0) throw new Error('Invalid dashboard count.');
    const status = group.status || 'Unknown';
    const severity = group.severity || 'Unassessed';
    statuses[status] = (statuses[status] || 0) + group.count;
    severities[severity] = (severities[severity] || 0) + group.count;
    total += group.count;
    if (status === 'InReview' && ['Critical', 'High'].includes(severity)) urgentReview += group.count;
  }
  for (const day of data.trend) {
    if (!/^\d{4}-\d{2}-\d{2}$/.test(day.date) || !Number.isInteger(day.count) || day.count < 0) throw new Error('Invalid dashboard trend.');
  }
  return { ...data, statuses, severities, total, urgentReview };
}

// Quote every cell and neutralize spreadsheet formulas, including unexpected API labels.
const cell = value => `"${String(value).replace(/^[=+@\-\t\r]/, "'$&").replaceAll('"', '""')}"`;
export function dashboardCsv(summary) {
  const rows = [
    ['CareFlow AI - Doctor dashboard summary'],
    ['Generated at (UTC)', summary.generatedAt],
    ['Scope', 'All cases visible in the doctor review queue'],
    ['Period', 'All-time totals; daily submissions cover the last seven UTC calendar days'],
    [], ['Category', 'Metric', 'Count'],
    ['Overview', 'Total submissions', summary.total],
    ['Overview', 'High / critical awaiting review', summary.urgentReview],
    ...Object.entries(summary.statuses).map(([key, count]) => ['Status', statusLabels[key] || key, count]),
    ...Object.entries(summary.severities).map(([key, count]) => ['Urgency', key, count]),
    ...summary.trend.map(day => ['Daily submissions (UTC)', day.date, day.count]),
  ];
  return '\uFEFF' + rows.map(row => row.map(cell).join(',')).join('\r\n');
}
