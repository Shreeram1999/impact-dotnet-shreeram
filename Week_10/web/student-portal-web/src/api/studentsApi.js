import { api } from './client';

export async function getStudents() {
  const { data } = await api.get('/academics/api/students');
  return data;
}

export async function createStudent(student) {
  const { data } = await api.post('/academics/api/students', student);
  return data;
}

export async function updateStudent(id, student) {
  await api.put(`/academics/api/students/${id}`, student);
  return { ...student, id };
}

export async function deleteStudent(id) {
  await api.delete(`/academics/api/students/${id}`);
}
