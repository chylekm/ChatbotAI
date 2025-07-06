import { MessageRole } from "../enums/message-role";

export interface Message {
  id?: string;
  conversationId?: string;
  role: MessageRole;
  text: string;
  isPartial?: boolean;
  rating?: number | null;
};