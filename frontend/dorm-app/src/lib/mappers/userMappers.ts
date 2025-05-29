import { UserProfileProps } from "@/components/UserProfile";
import { UserDetails } from "../hooks/useUser";

export function mapUserDetailsToUserProfileProps(
  details: UserDetails,
): UserProfileProps {
  console.log(details);
  return {
    user: {
      // з'єднуємо firstName + lastName
      name: `${details.firstName} ${details.lastName}`,
      email: details.email,
      // беремо першу роль або дефолт
      role: details.roles[0] ?? "Guest",

      // якщо є dormInfo – беремо звідти, інакше дефолт
      room: details.dormInfo?.room ?? "—",
      floor: details.dormInfo?.floor ?? "—",
      building: details.dormInfo?.building ?? "—",

      // кількість балів (якщо немає – 0)
      points: details.points ?? 0,

      // URL аватарки або плейсхолдер
      avatar: details.avatarUrl,
    },
    userId: details.id,
  };
}
