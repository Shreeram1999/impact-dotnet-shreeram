import { api } from './client';

// Week 10 - the Reporting service, reached through the gateway's
// /reporting prefix.
export async function getTerms() {
  const { data } = await api.get('/reporting/api/reports/terms');
  return data;
}

export async function getEnrollmentSummary(termId) {
  const { data } = await api.get('/reporting/api/reports/enrollment-summary', {
    params: termId ? { termId } : {},
  });
  return data;
}
