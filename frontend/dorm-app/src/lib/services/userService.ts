import { apiRequest } from "../api";
import { AvatarUploadResponse, UserProfile } from "../types/auth";

class UserService {
  /**
   * Get current user profile
   */
  async getCurrentUserProfile(): Promise<UserProfile> {
    return await apiRequest<UserProfile>("/api/user/profile");
  }

  /**
   * Upload user avatar
   */
  async uploadAvatar(file: File): Promise<AvatarUploadResponse> {
    const accessToken = localStorage.getItem('accessToken');
    
    if (!accessToken) {
      throw new Error('No access token found');
    }

    const formData = new FormData();
    formData.append('avatarFile', file);

    const API_BASE_URL = import.meta.env.VITE_API_GATEWAY_URL || '';
    
    const response = await fetch(`${API_BASE_URL}/api/user/avatar`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${accessToken}`,
      },
      body: formData,
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: 'Upload failed' }));
      throw new Error(errorData.message || 'Failed to upload avatar');
    }

    return await response.json();
  }

  /**
   * Delete user avatar
   */
  async deleteAvatar(): Promise<void> {
    await apiRequest<void>("/api/user/avatar", {
      method: 'DELETE',
    });
  }
}

export const userService = new UserService(); 