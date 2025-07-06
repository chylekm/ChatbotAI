import { Injectable } from "@angular/core";
import { MessageRole } from "../enums/message-role";
import { Message } from "../models/message.interface";
import { ChatService } from "./chat.service";

@Injectable({ providedIn: 'root' })
export class RateService {
  constructor(private api: ChatService) {}

  rate(message: Message, value: number, update: (message: Message) => void): void {
    if (!message.id || message.role !== MessageRole.AI) return;

    const newRating = message.rating === value || value === 0 ? null : value;
    message.rating = newRating;

    this.api.patchRating(message.id, newRating).subscribe({
      error: () => {
        message.rating = null;
        update(message);
        console.error('Rating failed.');
      }
    });

    update(message);
  }
}
