export const sample1 = `Medical Leave and Leave of Absence Policy
Company XYZ

Medical leave is a type of leave that allows employees to take time off from work for health-related reasons. Employees may request medical leave for themselves or for their immediate family members who need their care. Medical leave may be paid or unpaid, depending on the employee's eligibility and the nature of the leave.

Leave of absence is a type of leave that allows employees to take time off from work for personal or professional reasons that are not related to health. Employees may request a leave of absence for reasons such as education, family care, military service, civic duty, or personal growth. Leave of absence may be paid or unpaid, depending on the employee's eligibility and the nature of the leave.

Employees who wish to request medical leave or leave of absence must follow these steps:

- Notify their supervisor and the human resources department as soon as possible about their need for leave and the expected duration.
- Provide appropriate documentation to support their request, such as a doctor's note, a certificate of enrollment, a court summons, or a military order.
- Complete and submit the required forms to the human resources department before the start of their leave.
- Keep in touch with their supervisor and the human resources department during their leave and inform them of any changes in their situation or return date.
- Return to work on the agreed date and resume their duties and responsibilities.
- Employees who take medical leave or leave of absence are entitled to certain benefits and protections, such as:

Continuation of their health insurance coverage and other benefits during their leave, subject to the terms and conditions of the benefit plans.

- Restoration of their original or equivalent position upon their return, unless their position is eliminated due to business reasons or they are unable to perform their essential functions.
- Protection from discrimination, harassment, retaliation, or interference for exercising their right to take leave.

Employees who take medical leave or leave of absence are expected to comply with certain obligations and expectations, such as:

- Using their leave for the intended purpose and not engaging in any activities that are inconsistent with their reason for leave.
- Following the company's policies and procedures regarding attendance, performance, conduct, and communication during their leave.
- Cooperating with the company's requests for information or verification regarding their leave status or eligibility.
- Returning any company property or equipment that they have in their possession before or during their leave.

The company reserves the right to modify or terminate this policy at any time, with or without notice. This policy does not create a contract or guarantee of employment. This policy is subject to applicable federal, state, and local laws and regulations. If there is any conflict between this policy and the law, the law will prevail.`;

export const sample2 = `Company Benefits Policy
Company XYZ


Introduction
At Company XYZ, we believe that our employees are our most valuable asset. We are committed to providing a comprehensive and competitive benefits package to attract and retain top talent. This policy outlines the various benefits available to our employees, including paid vacation, paid insurance and dental, 401K, and others.

Paid Vacation
We recognize the importance of work-life balance and understand that employees need time off to relax, recharge, and spend quality time with their loved ones. Therefore, we offer a generous paid vacation policy to all eligible employees. The specific details of the paid vacation policy, including accrual rates, eligibility criteria, and request procedures, will be outlined in the Employee Handbook.

Paid Insurance and Dental
We understand the significance of health and dental care in maintaining the well-being of our employees. Therefore, we provide comprehensive health insurance coverage and dental benefits to eligible employees and their dependents. The specific details of the insurance and dental coverage, including eligibility criteria, coverage limits, and claim procedures, will be outlined in the Employee Handbook.

401K Retirement Plan
We are committed to helping our employees plan for their future and achieve financial security. To support this, we offer a 401K retirement plan to all eligible employees. Through this plan, employees can contribute a portion of their pre-tax income, and the company may provide matching contributions based on a predetermined formula. The specific details of the 401K retirement plan, including eligibility criteria, contribution limits, and vesting schedules, will be outlined in the Employee Handbook.

Other Benefits
In addition to the aforementioned benefits, we offer a range of other benefits to enhance the overall employee experience. These benefits may include, but are not limited to:

a. Flexible Work Arrangements: We understand that employees have different needs and responsibilities outside of work. Therefore, we strive to provide flexible work arrangements, such as telecommuting or flexible hours, whenever possible and appropriate.

b. Employee Assistance Program (EAP): We recognize that personal and professional challenges can arise, impacting an employee's well-being and productivity. Our EAP provides confidential counseling and support services to help employees navigate through difficult times.

c. Wellness Programs: We promote a healthy lifestyle and encourage our employees to prioritize their well-being. We may offer wellness programs, such as gym memberships, fitness classes, or wellness challenges, to support employees in maintaining a healthy work-life balance.

d. Professional Development: We believe in fostering continuous learning and growth. Therefore, we may provide opportunities for professional development, such as training programs, workshops, or tuition reimbursement, to help employees enhance their skills and advance their careers.

e. Employee Recognition Programs: We value the hard work and dedication of our employees. To acknowledge their contributions, we may implement employee recognition programs, such as employee of the month/year awards, spot bonuses, or team-building activities.

Communication and Administration:

We understand the importance of clear communication and efficient administration of our benefits program. Therefore, we will provide employees with detailed information about their benefits, including eligibility requirements, enrollment procedures, and any changes or updates to the benefits program. This information will be communicated through various channels, such as employee handbooks, company intranet, or direct communication from the HR department.

Compliance with Applicable Laws and Regulations:

We are committed to complying with all applicable laws and regulations governing employee benefits, including but not limited to the Employee Retirement Income Security Act (ERISA), the Affordable Care Act (ACA), and the Health Insurance Portability and Accountability Act (HIPAA). We will regularly review and update our benefits policies and procedures to ensure compliance with these laws and regulations.

Modification or Termination of Benefits:

While we strive to provide a comprehensive benefits package, we reserve the right to modify or terminate any benefits at our discretion. Any changes to the benefits program will be communicated to employees in a timely manner, and we will make reasonable efforts to minimize any adverse impact on employees.

Conclusion
At Company XYZ, we are committed to providing a competitive and comprehensive benefits package to support the well-being and satisfaction of our employees. This policy serves as a guideline for the benefits we offer, and we will continue to review and enhance our benefits program to meet the evolving needs of our workforce.

Please note that this policy is not intended to create a contractual obligation between Company XYZ and its employees, and the company reserves the right to interpret and administer the benefits program in its sole discretion.`

// export const samplePrompt = `System:
// You are a corporate HR knowledge assistant. Answer the user's questions with the provided text only. If you cannot answer the question say, "I do not have this information." After every interaction say, "NOTE: Please make sure to follow proper procedures and documentation."

// User:
// Text: """
// <CONTEXT>
// """

// What are some company benefits?`

export const samplePrompt = `In one paragraph, what are some company benefits?`

export function uuidv4() {
  let userId = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'
    .replace(/[xy]/g, function (c) {
      const r = Math.random() * 16 | 0,
        v = c == 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  userId = userId.replace(/-/g, '')
  return "user_" + userId.substring(userId.length - 12)
}