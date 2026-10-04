// Client-side copies of the API's validation rules, so forms can show
// problems inline and keep Submit disabled until the input is valid. The
// API re-checks everything (DataAnnotations) - these are for the user's
// convenience, not for security.

export const PASSWORD_RULE = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,100}$/;
const EMAIL_RULE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function validateRegistration(form, today = new Date()) {
  const errors = {};

  if (form.name.trim().length < 2) {
    errors.name = 'Name must be at least 2 characters.';
  }

  if (!form.dateOfBirth) {
    errors.dateOfBirth = 'Date of birth is required.';
  } else {
    const latestAllowed = new Date(today);
    latestAllowed.setFullYear(latestAllowed.getFullYear() - 5);
    if (new Date(form.dateOfBirth) > latestAllowed) {
      errors.dateOfBirth = 'Date of birth must be at least 5 years ago.';
    }
  }

  if (!['Student', 'Teacher'].includes(form.designation)) {
    errors.designation = 'Choose Student or Teacher.';
  }

  if (!EMAIL_RULE.test(form.email.trim())) {
    errors.email = 'Enter a valid email address.';
  }

  if (!PASSWORD_RULE.test(form.password)) {
    errors.password = 'Use 8+ characters with upper case, lower case and a digit.';
  }

  return errors;
}

export function validateStudent(form) {
  const errors = {};
  const age = Number(form.age);
  const score = Number(form.score);

  if (!form.name.trim()) errors.name = 'Name is required.';
  else if (form.name.length > 100) errors.name = 'Name must be 100 characters or fewer.';

  if (form.age === '' || !Number.isInteger(age) || age < 5 || age > 100) {
    errors.age = 'Age must be a whole number from 5 to 100.';
  }

  if (!form.rollNumber.trim()) errors.rollNumber = 'Roll number is required.';
  else if (form.rollNumber.length > 20) errors.rollNumber = 'Roll number must be 20 characters or fewer.';

  if (!EMAIL_RULE.test(form.email.trim())) errors.email = 'Enter a valid email address.';

  if (form.score === '' || !Number.isInteger(score) || score < 0 || score > 100) {
    errors.score = 'Score must be a whole number from 0 to 100.';
  }

  return errors;
}

export function isValid(errors) {
  return Object.keys(errors).length === 0;
}
