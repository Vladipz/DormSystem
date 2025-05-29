"use client";

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  AchievementsTab,
  ActivityTab,
  ProfileTab,
  SettingsTab,
  UserInfoCard,
} from "@/components/user-profile";
import { UserDetails } from "@/lib/hooks/useUser";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";

export interface UserProfileProps {
  user: {
    name: string;
    email: string;
    role: string;
    room: string;
    floor: string;
    building: string;
    points: number;
    avatar: string;
  };
  userId: string;
}

export function UserProfile({ user, userId }: UserProfileProps) {
  const [name, setName] = useState(user.name);
  const [email, setEmail] = useState(user.email);
  const [phone, setPhone] = useState("+380 12 345 6789");
  const [bio, setBio] = useState(
    "Computer Science student. I enjoy playing basketball and video games.",
  );
  const queryClient = useQueryClient();

  // Синхронізуємо локальний стан з props при їх зміні
  useEffect(() => {
    setName(user.name);
    setEmail(user.email);
  }, [user.name, user.email]);

  const handleAvatarUpdate = async (newAvatarUrl: string) => {
    console.log("Avatar update received:", newAvatarUrl);

    // Спочатку оновлюємо кеш з правильною структурою
    queryClient.setQueryData<UserDetails>(["user", userId], (oldData) => {
      if (oldData) {
        console.log("Updating cache with new avatar:", newAvatarUrl);
        return {
          ...oldData,
          avatar: newAvatarUrl, // Використовуємо avatar, а не avatarUrl
          avatarUrl: newAvatarUrl, // Якщо потрібно для сумісності
        };
      }
      return oldData;
    });

    // Потім інвалідуємо для отримання свіжих даних з сервера
    setTimeout(async () => {
      try {
        await queryClient.invalidateQueries({
          queryKey: ["user", userId],
          exact: true,
        });
        console.log("Query invalidated successfully");
      } catch (error) {
        console.error("Failed to invalidate query:", error);
      }
    }, 100); // Невелика затримка для уникнення race condition
  };

  // Mock achievements
  const achievements = [
    {
      id: 1,
      title: "Event Organizer",
      description: "Organized 3 dormitory events",
      date: "2023-06-15",
    },
    {
      id: 2,
      title: "Helpful Neighbor",
      description: "Received 5 positive reviews from neighbors",
      date: "2023-05-20",
    },
    {
      id: 3,
      title: "Eco Warrior",
      description: "Participated in dormitory recycling program",
      date: "2023-04-10",
    },
  ];

  // Mock activity history
  const activityHistory = [
    { id: 1, type: "Event", title: "Joined Movie Night", date: "2023-07-10" },
    { id: 2, type: "Booking", title: "Booked Gym", date: "2023-07-08" },
    { id: 3, type: "Laundry", title: "Booked Washer 1", date: "2023-07-05" },
    {
      id: 4,
      type: "Purchase Request",
      title: "Voted for New Treadmill",
      date: "2023-07-02",
    },
    {
      id: 5,
      type: "Marketplace",
      title: "Listed Desk Lamp for Sale",
      date: "2023-06-28",
    },
  ];

  return (
    <div className="space-y-6">
      <UserInfoCard
        user={user}
        bio={bio}
        onAvatarUpdate={handleAvatarUpdate}
      />

      <Tabs defaultValue="profile">
        <TabsList>
          <TabsTrigger value="profile">Profile</TabsTrigger>
          <TabsTrigger value="achievements">Achievements</TabsTrigger>
          <TabsTrigger value="activity">Activity</TabsTrigger>
          <TabsTrigger value="settings">Settings</TabsTrigger>
        </TabsList>

        <TabsContent value="profile">
          <ProfileTab
            name={name}
            email={email}
            phone={phone}
            roomNumber={user.room}
            bio={bio}
            setName={setName}
            setEmail={setEmail}
            setPhone={setPhone}
            setBio={setBio}
          />
        </TabsContent>

        <TabsContent value="achievements">
          <AchievementsTab achievements={achievements} />
        </TabsContent>

        <TabsContent value="activity">
          <ActivityTab activities={activityHistory} />
        </TabsContent>

        <TabsContent value="settings">
          <SettingsTab userId={userId} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
