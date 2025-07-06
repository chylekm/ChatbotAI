import {
  Component,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  OnDestroy,
  signal,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SHARED_MATERIAL_IMPORTS } from '../../../../shared/shared-material';
import { MessageRole } from '../../enums/message-role';
import { Message } from '../../models/message.interface';
import { ApiService } from '../../services/api.service';
import { Subscription } from 'rxjs';
import { ScrollService } from '../../../../shared/services/scroll-service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ...SHARED_MATERIAL_IMPORTS
  ],
  templateUrl: './chat-component.html',
  styleUrl: './chat-component.scss'
})
export class ChatComponent implements OnDestroy {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;

  private readonly destroyRef = inject(DestroyRef);
  private readonly scrollService = inject(ScrollService);
  constructor(private chatService: ApiService) {
    effect(
      () => {
        this.messages();
        this.scrollService.scrollToBottom(this.messagesContainer);
      }
    );
  }

  readonly messages = signal<Message[]>([]);
  readonly loading = signal(false);

  readonly messageRole = MessageRole;

  aiStreamSubscription?: Subscription;
  currentStream = signal('');
  aiMessageId?: string;

  form = new FormGroup({
    message: new FormControl('', [Validators.required])
  });

  get message(): string | null | undefined {
    return this.form.get("message")?.value;
  }

  ngOnInit(): void {
    this.form.get('message')!.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }


  ngOnDestroy(): void {
    this.cancelGeneration();
  }

  rate(message: Message, value: number): void {
    if (!message.id || message.role !== MessageRole.AI) return;

    const newRating = message.rating === value || value == 0 ? null : value;
    message.rating = newRating;
    this.messages.set([...this.messages()]);

    this.chatService.patchRating(message.id, newRating).subscribe({
      error: () => {
        message.rating = null;
        this.messages.set([...this.messages()]);
        console.error('Rating failed.');
      }
    });
  }

  sendMessage(): void {
    if (!this.canSendMessage()) return;

    const text = this.getMessageText();
    this.appendUserMessage(text);
    this.prepareAiResponse();

    this.aiStreamSubscription = this.chatService.streamAiResponse(text).subscribe({
      next: ({ chunk, messageId }) => {
        this.assignMessageIdIfNeeded(messageId);
        this.updateAiMessage(chunk);
      },
      error: () => this.finalizeAiMessage(),
      complete: () => this.finalizeAiMessage()
    });
  }


  cancelGeneration(): void {
    if (!this.loading()) return;

    this.aiStreamSubscription?.unsubscribe();
    this.finalizeAiMessage();
  }

  formatText(text: string): string {
    return text
      .split(/\n{2,}/)
      .map(paragraph => `<p>${paragraph.trim()}</p>`)
      .join('');
  }

  private canSendMessage(): boolean {
    return !this.loading() && this.form.valid && !!this.getMessageText();
  }

  private getMessageText(): string {
    return this.form.controls.message.value?.trim() ?? '';
  }

  private appendUserMessage(text: string): void {
    const userMessage: Message = {
      role: MessageRole.User,
      text
    };
    this.messages.update(messages => [...messages, userMessage]);
  }

  private prepareAiResponse(): void {
    this.loading.set(true);
    this.form.reset();

    const aiMessage: Message = {
      role: MessageRole.AI,
      text: '',
      isPartial: true
    };
    this.messages.update(messages => [...this.messages(), aiMessage]);
  }

  private assignMessageIdIfNeeded(messageId?: string): void {
    if (!messageId || this.aiMessageId) return;

    this.aiMessageId = messageId;
    const current = this.messages();
    const last = current.at(-1);

    if (last?.role === MessageRole.AI) {
      last.id = messageId;
      this.messages.set([...current.slice(0, -1), last]);
    }
  }

  private updateAiMessage(chunk: string): void {
    if (!chunk) return;

    const messages = this.messages();
    const last = messages.at(-1);

    if (last?.role === MessageRole.AI && last.isPartial) {
      last.text += chunk;
      this.messages.set([...messages.slice(0, -1), last]);
    }
  }

  private finalizeAiMessage(): void {
    const current = this.messages();
    const last = current.at(-1);

    if (last?.role === MessageRole.AI) {
      last.isPartial = false;
      this.messages.set([...current.slice(0, -1), last]);
    }

    this.form.reset();
    this.loading.set(false);
    this.aiStreamSubscription = undefined;
    this.currentStream.set('');
    this.aiMessageId = undefined;
  }
}