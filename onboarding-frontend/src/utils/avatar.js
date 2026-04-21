export const getAvatarGradient = (name) => {
  if (!name) return 'linear-gradient(135deg, #f97316, #ec4899, #8b5cf6)';
  const gradients = [
    'linear-gradient(135deg, #f97316, #ec4899, #8b5cf6)', // orange-pink-purple
    'linear-gradient(135deg, #3b82f6, #2dd4bf, #10b981)', // blue-teal-emerald
    'linear-gradient(135deg, #f43f5e, #f59e0b, #84cc16)', // rose-amber-lime
    'linear-gradient(135deg, #8b5cf6, #3b82f6, #0ea5e9)', // purple-blue-sky
    'linear-gradient(135deg, #ec4899, #f43f5e, #f97316)', // pink-rose-orange
    'linear-gradient(135deg, #14b8a6, #3b82f6, #8b5cf6)', // teal-blue-purple
    'linear-gradient(135deg, #0ea5e9, #10b981, #f59e0b)', // sky-emerald-amber
    'linear-gradient(135deg, #6366f1, #a855f7, #ec4899)'  // indigo-purple-pink
  ];
  let sum = 0;
  for (let i = 0; i < name.length; i++) sum += name.charCodeAt(i);
  return gradients[sum % gradients.length];
};
