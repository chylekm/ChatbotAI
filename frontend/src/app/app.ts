import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatToolbar } from '@angular/material/toolbar';
import { SHARED_MATERIAL_IMPORTS } from './shared/shared-material';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, 
    ...SHARED_MATERIAL_IMPORTS
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  public title = 'ChatbotAI';
}
