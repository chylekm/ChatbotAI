import {
  Component,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  OnDestroy,
  ViewChild,
  OnInit,
  signal
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SHARED_MATERIAL_IMPORTS } from '../../../../shared/shared-material';
import { MessageRole } from '../../enums/message-role';
import { Message } from '../../models/message.interface';
import { ScrollService } from '../../../../shared/services/scroll-service';
import { MessageService } from '../../services/message.service';
import { ChatService } from '../../services/chat.service';

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
export class ChatComponent implements OnDestroy, OnInit {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;

  private readonly destroyRef = inject(DestroyRef);
  private readonly scrollService = inject(ScrollService);

  constructor(
    private messageService: MessageService,
    private chatService: ChatService
  ) {
    effect(() => {
      this.messages();
      this.scrollService.scrollToBottom(this.messagesContainer);
    });
  }

  readonly messages = signal<Message[]>([]);
  readonly loading = signal(false);
  readonly currentStream = signal('');
  readonly messageRole = MessageRole;

  aiMessageId?: string;

  form = new FormGroup({
    message: new FormControl('', [Validators.required, Validators.maxLength(500)])
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

  sendMessage(): void {
    if (!this.canSendMessage()) return;

    const text = this.getMessageText();

    this.messageService.sendMessage(
      text,
      this.messages,
      this.messages.set.bind(this.messages),
      this.loading.set.bind(this.loading),
      (id) => this.aiMessageId = id,
      () => this.currentStream.set('')
    );

    this.form.reset();
  }

  cancelGeneration(): void {
    if (!this.loading()) return;

    this.messageService.cancelGeneration(
      this.loading.set.bind(this.loading),
      this.messages,
      this.messages.set.bind(this.messages)
    );

    this.form.reset();          
    this.currentStream.set('');
    this.aiMessageId = undefined;
  }

  rate(message: Message, value: number): void {
    if (!message.id || message.role !== MessageRole.AI) return;

    const newRating = message.rating === value || value === 0 ? null : value;
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
}
