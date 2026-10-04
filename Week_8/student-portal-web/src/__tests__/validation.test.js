import { isValid, validateRegistration, validateStudent } from '../validation';

const today = new Date('2026-10-01T12:00:00Z');
const validRegistration = { name: 'Asha', dateOfBirth: '2005-05-20', designation: 'Student', email: 'asha@school.example', password: 'Passw0rd' };
const validStudent = { name: 'Asha', age: '20', rollNumber: 'R1', email: 'asha@school.example', score: '82' };

describe('validateRegistration', () => {
  test('a complete, valid form has no errors', () => {
    expect(isValid(validRegistration ? validateRegistration(validRegistration, today) : {})).toBe(true);
  });

  test.each([
    ['name', 'A', 'name'],
    ['dateOfBirth', '', 'dateOfBirth'],
    ['dateOfBirth', '2030-01-01', 'dateOfBirth'],
    ['dateOfBirth', '2022-01-01', 'dateOfBirth'],
    ['designation', 'Admin', 'designation'],
    ['email', 'asha@', 'email'],
    ['password', 'password1', 'password'],
    ['password', 'PASSWORD1', 'password'],
    ['password', 'Passwordx', 'password'],
    ['password', 'Pa55', 'password'],
  ])('%s = %p is rejected', (field, value, errorKey) => {
    const errors = validateRegistration({ ...validRegistration, [field]: value }, today);

    expect(errors[errorKey]).toBeDefined();
    expect(isValid(errors)).toBe(false);
  });

  test('defaults "today" to the real date', () => {
    expect(validateRegistration(validRegistration)).toEqual({});
  });
});

describe('validateStudent', () => {
  test('a valid student has no errors', () => {
    expect(validateStudent(validStudent)).toEqual({});
  });

  test.each([
    ['name', ''],
    ['name', 'x'.repeat(101)],
    ['age', ''],
    ['age', '4'],
    ['age', '101'],
    ['age', '20.5'],
    ['rollNumber', ' '],
    ['rollNumber', 'R'.repeat(21)],
    ['email', 'nope'],
    ['score', ''],
    ['score', '-1'],
    ['score', '101'],
  ])('%s = %p is rejected', (field, value) => {
    expect(validateStudent({ ...validStudent, [field]: value })[field]).toBeDefined();
  });

  test('boundaries 5/100 for age and 0/100 for score are accepted', () => {
    expect(validateStudent({ ...validStudent, age: '5', score: '0' })).toEqual({});
    expect(validateStudent({ ...validStudent, age: '100', score: '100' })).toEqual({});
  });
});
