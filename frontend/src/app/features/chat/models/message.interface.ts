import { MessageRole } from "../enums/message-role";
import { Rating } from "../enums/rating.enum";

export interface Message {
  id?: string;
  conversationId?: string;
  role: MessageRole;
  text: string;
  isPartial?: boolean;
  rating?: number | null;
};