import { Component, ElementRef, ViewChild } from '@angular/core';
import { SHARED_MATERIAL_IMPORTS } from '../../../../shared/shared-material';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ChatMessage } from '../../models/chat-message.interface';
import { UserRole } from '../../enums/user-role';
import { Rating } from '../../enums/rating.enum';

@Component({
  selector: 'app-chat-component',
  imports: [...SHARED_MATERIAL_IMPORTS, CommonModule, ReactiveFormsModule],
  templateUrl: './chat-component.html',
  styleUrl: './chat-component.scss'
})
export class ChatComponent {

  @ViewChild('historyContainer') historyContainer!: ElementRef<HTMLDivElement>;
  userRole = UserRole;
  rating = Rating;
  chatForm = new FormGroup({
      message: new FormControl('', Validators.required)
    });

  messages: ChatMessage[] = [];
  botIsTyping = false;
  botResponseTimeout: any;

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  sendMessage() {
    if (this.chatForm.invalid) return;

    const message = this.chatForm.value.message!.trim();
    this.messages.push({ role: UserRole.User, text: message });
    this.chatForm.reset();
    this.botIsTyping = true;

    this.botResponseTimeout = setTimeout(() => {
      const response = this.getFakeBotResponse();
      this.messages.push({ role: UserRole.Bot, text: response });
      this.botIsTyping = false;
    }, 1500);
  }

  cancelGeneration() {
    clearTimeout(this.botResponseTimeout);
    this.botIsTyping = false;
    this.messages.push({ role: UserRole.Bot, text: '[ODPOWIEDŹ PRZERWANA]' });
  }

  getFakeBotResponse(): string {
    const responses = [
      'Lorem ipsum dolor sit amet.',
      'Vestibulum ante ipsum primis in faucibus.',
      'Curabitur non nulla sit amet nisl tempus.',
      'Sed porttitor lectus nibh.',
      'Pellentesque in ipsum id orci porta dapibus.',
    ];
    const randomIndex = Math.floor(Math.random() * responses.length);
    return responses[randomIndex];
  }

  rateMessage(message: ChatMessage, value: Rating) {
    if (message.rating === value) {
      message.rating = null;
    } else {
      message.rating = value;
    }
    // TODO: zapisz ocenę do backendu (np. PUT /api/chat/rate)
  }

  private scrollToBottom() {
    if (this.historyContainer) {
      try {
        this.historyContainer.nativeElement.scrollTop =
          this.historyContainer.nativeElement.scrollHeight;
      } catch (err) {
        console.warn('Auto-scroll failed:', err);
      }
    }
  }
}
