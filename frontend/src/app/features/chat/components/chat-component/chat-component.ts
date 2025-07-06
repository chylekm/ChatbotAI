import {
  Component,
  DestroyRef,
  ElementRef,
  inject,
  signal,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, FormGroup, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { SHARED_MATERIAL_IMPORTS } from '../../../../shared/shared-material';
import { Rating } from '../../enums/rating.enum';
import { MessageRole } from '../../enums/message-role';
import { Message } from '../../models/message.interface';
import { ChatService } from '../../services/chat.service';
import { Subscription } from 'rxjs';

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
export class ChatComponent {

  private readonly destroyRef = inject(DestroyRef);

  // Wstrzyknięcie ChatService w konstruktorze – bez błędów DI
  constructor(private chatService: ChatService) { }

  readonly messages = signal<Message[]>([]);
  readonly loading = signal(false);

  readonly messageRole = MessageRole;
  readonly rating = Rating;

  aiStreamSubscription?: Subscription;
  currentStream = signal('');

  form = new FormGroup({
    message: new FormControl('', [Validators.required, Validators.maxLength(2000)])
  });

  ngOnInit(): void {
    this.form.get('message')!.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }

  ngOnDestroy(): void {
    this.cancelGeneration();
  }

  rate(message: Message, value: number): void {
    message.rating = value;
    this.messages.set([...this.messages()]);
    // TODO: api/chat/rate
  }

  sendMessage(): void {
    const value = this.form.get('message')!.value;
    if (!value || this.form.invalid) return;

    const userMessage: Message = {
      role: MessageRole.User,
      text: value
    };

    this.messages.update(messages => [...messages, userMessage]);
    this.loading.set(true);
    this.form.reset();

    const aiMessage: Message = {
      role: MessageRole.AI,
      text: '',
      isPartial: true
    };

    this.messages.update(msgs => [...msgs, aiMessage]);

    this.aiStreamSubscription = this.chatService.streamAiResponse(value).subscribe({
      next: (chunk) => this.updateAiMessage(chunk),
      error: () => this.finalizeAiMessage(),
      complete: () => this.finalizeAiMessage()
    });
  }

  cancelGeneration(): void {
    this.aiStreamSubscription?.unsubscribe();
    this.finalizeAiMessage();
  }
 
  private updateAiMessage(chunk: string): void {
    const current = this.messages();
    const last = current.at(-1);

    if (last?.role === MessageRole.AI && last.isPartial) {
      last.text += chunk;
      this.messages.set([...current.slice(0, -1), last]);
    }
  }
  private finalizeAiMessage(): void {
    const current = this.messages();
    const last = current.at(-1);

    if (last?.role === MessageRole.AI) {
      last.isPartial = false;
      this.messages.set([...current.slice(0, -1), last]);
    }

    this.loading.set(false);
    this.aiStreamSubscription = undefined;
    this.currentStream.set('');
  }
}