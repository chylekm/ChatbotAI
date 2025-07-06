import { inject, Injectable } from '@angular/core';
import { defer, Observable, Observer } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class ApiService {

  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/chat`;

  patchRating(messageId: string, rating: number | null): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/rate`, {
      messageId,
      rating
    });
  }

  streamAiResponse(message: string, conversationId?: string): Observable<{ chunk: string; messageId?: string }> {
    return new Observable(observer => {
      const controller = new AbortController();

      this.sendStreamRequest(message, conversationId, controller.signal)
        .then(response => this.readStream(response, observer))
        .catch(error => observer.error(error));

      // funkcja czyszcząca: wywoływana przy unsubscribe
      return () => controller.abort();
    });
  }

  private sendStreamRequest(message: string, conversationId: string | undefined, signal: AbortSignal): Promise<Response> {
    return fetch(`${this.baseUrl}/stream`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'text/event-stream'
      },
      body: JSON.stringify({ message, conversationId }),
      signal
    });
  }

  private readStream(response: Response, observer: Observer<{ chunk: string; messageId?: string }>): void {
    if (!response.body) {
      observer.error('No content in response');
      return;
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder('utf-8');
    let messageId: string | undefined;

    const read = () => {
      reader.read().then(({ done, value }) => {
        if (done) {
          observer.complete();
          return;
        }

        const text = decoder.decode(value, { stream: true });

        if (text.startsWith('id:')) {
          messageId = text.replace('id:', '').trim();
          observer.next({ chunk: '', messageId });
        } else {
          observer.next({ chunk: text, messageId });
        }

        read();
      }).catch(error => observer.error(error));
    };

    read();
  }

}
