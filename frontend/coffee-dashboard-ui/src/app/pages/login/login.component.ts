import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Title } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { ApiService } from '../../api.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  username = '';
  password = '';
  loading = false;
  error = '';

  constructor(
    private readonly router: Router,
    private readonly apiService: ApiService,
    private readonly title: Title
  ) {
    this.title.setTitle('marketboard');
  }

  async ngOnInit(): Promise<void> {
    // NOTE: Auto-login bằng admin/admin để bỏ qua trang login.
    // Nếu đổi password, auto-login sẽ fail và user ở lại trang login.
    if (!this.username && !this.password) {
      this.username = 'admin';
      this.password = 'admin';
      await this.login();
      // Uncomment để rollback về login thủ công.
      // this.username = '';
      // this.password = '';
    }
  }

  async login(): Promise<void> {
    this.loading = true;
    this.error = '';

    try {
      const response = await this.apiService.login(this.username, this.password);
      localStorage.setItem('coffee-dashboard-auth', response.accessToken);
      await this.router.navigateByUrl('/dashboard');
    } catch {
      this.error = 'Đăng nhập thất bại. Kiểm tra backend hoặc tài khoản demo.';
    } finally {
      this.loading = false;
    }
  }
}
