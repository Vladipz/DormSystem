import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/**
 * Generates a random emoji to be used as a placeholder avatar
 * Can be easily replaced with actual user avatars later
 * 
 * @param index - Optional index to consistently show the same emoji for the same position
 * @param seed - Optional string to use as a seed to consistently show the same emoji for the same entity
 * @returns An emoji character
 */
export function getPlaceholderAvatar(index?: number, seed?: string): string {
  // Array of emojis for random selection
  const emojis = ["😀", "😎", "🥳", "🤩", "🦸", "🧑‍💻", "🧠", "👾", "🐱", "🦊", "🐻", "🐼", "🦁", "🐯", "🐶", "🐵"];
  
  if (typeof index === 'number') {
    // Use index to deterministically select an emoji
    return emojis[Math.abs(index) % emojis.length];
  } 
  
  if (seed) {
    // Generate a hash from the seed string to deterministically select an emoji
    let hashValue = 0;
    for (let i = 0; i < seed.length; i++) {
      hashValue = ((hashValue << 5) - hashValue) + seed.charCodeAt(i);
      hashValue = hashValue & hashValue; // Convert to 32bit integer
    }
    return emojis[Math.abs(hashValue) % emojis.length];
  }
  
  // Fallback to random emoji
  return emojis[Math.floor(Math.random() * emojis.length)];
}
