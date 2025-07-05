import { Rating } from "../enums/rating.enum";
import { UserRole } from "../enums/user-role";

export interface ChatMessage {
  role: UserRole;
  text: string;
  rating?: Rating | null;
};