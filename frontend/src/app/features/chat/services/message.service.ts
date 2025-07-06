import { Injectable, Signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { ChatService } from './chat.service';
import { Message } from '../models/message.interface';
import { MessageRole } from '../enums/message-role';

@Injectable({ providedIn: 'root' })
export class MessageService {
  private aiStreamSubscription?: Subscription;

  constructor(private chatService: ChatService) { }

  sendMessage(
    text: string,
    messages: Signal<Message[]>,
    setMessages: (msgs: Message[]) => void,
    setLoading: (val: boolean) => void,
    setMessageId: (id: string | undefined) => void,
    onComplete: () => void
  ): void {
    const userMessage: Message = { role: MessageRole.User, text };
    const aiMessage: Message = { role: MessageRole.AI, text: '', isPartial: true };

    setMessages([...messages(), userMessage, aiMessage]);
    setLoading(true);

    this.aiStreamSubscription = this.chatService.streamAiResponse(text).subscribe({
      next: ({ chunk, messageId }) => {
        this.assignMessageIdIfNeeded(messageId, messages, setMessages, setMessageId);
        this.updateAiMessage(chunk, messages, setMessages);
      },
      error: () => this.finalizeAiMessage(messages, setMessages, setLoading, onComplete),
      complete: () => this.finalizeAiMessage(messages, setMessages, setLoading, onComplete)
    });
  }

  cancelGeneration(
    setLoading: (val: boolean) => void,
    messages: Signal<Message[]>,
    setMessages: (msgs: Message[]) => void
  ): void {
    this.aiStreamSubscription?.unsubscribe();
    this.finalizeAiMessage(messages, setMessages, setLoading, () => { });
  }

  private assignMessageIdIfNeeded(
    messageId: string | undefined,
    messages: Signal<Message[]>,
    setMessages: (msgs: Message[]) => void,
    setMessageId: (id: string) => void
  ): void {
    if (!messageId) return;

    const current = messages();
    const last = current.at(-1);

    if (last?.role === MessageRole.AI && !last.id) {
      last.id = messageId;
      setMessages([...current.slice(0, -1), last]);
      setMessageId(messageId);
    }
  }

  private updateAiMessage(
    chunk: string,
    messages: Signal<Message[]>,
    setMessages: (msgs: Message[]) => void
  ): void {
    if (!chunk) return;

    const current = messages();
    const last = current.at(-1);

    if (last?.role === MessageRole.AI && last.isPartial) {
      last.text += chunk;
      setMessages([...current.slice(0, -1), last]);
    }
  }

  private finalizeAiMessage(
    messages: Signal<Message[]>,
    setMessages: (msgs: Message[]) => void,
    setLoading: (val: boolean) => void,
    onComplete: () => void
  ): void {
    const current = messages();
    const last = current.at(-1);

    if (last?.role === MessageRole.AI) {
      last.isPartial = false;
      setMessages([...current.slice(0, -1), last]);
    }

    setLoading(false);
    this.aiStreamSubscription = undefined;
    onComplete();
  }
}
