import React, { useState } from 'react';
import { Form, Input, Button, Card, Typography, Space, message } from 'antd';
import { UserOutlined, LockOutlined, GlobalOutlined, SafetyOutlined } from '@ant-design/icons';
import api from '../services/api';

const { Title, Text } = Typography;

export const LoginPage: React.FC<{ onLoginSuccess: () => void }> = ({ onLoginSuccess }) => {
  const [loading, setLoading] = useState(false);

  const onFinish = async (values: any) => {
    setLoading(true);
    try {
      const response = await api.post('/Auth/login', values);
      localStorage.setItem('token', response.data.accessToken);
      localStorage.setItem('user', JSON.stringify(response.data.user));
      message.success('Giriş başarılı! Yönetim paneline yönlendiriliyorsunuz.');
      onLoginSuccess();
    } catch (err: any) {
      console.error('Giriş Hatası Detayı:', err);
      const errorMsg =
        err.response?.data?.message || err.response?.data?.Message || err.message || 'Giriş başarısız!';
      message.error(`Giriş başarısız: ${errorMsg}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #001529 0%, #003a8c 50%, #1677ff 100%)',
        padding: 20,
      }}
    >
      <Card
        style={{
          width: 440,
          borderRadius: 16,
          boxShadow: '0 20px 40px rgba(0, 0, 0, 0.3)',
          border: 'none',
          padding: '24px 16px',
        }}
      >
        <div style={{ textAlign: 'center', marginBottom: 32 }}>
          <div
            style={{
              width: 64,
              height: 64,
              borderRadius: '50%',
              background: '#e6f4ff',
              display: 'inline-flex',
              alignItems: 'center',
              justifyContent: 'center',
              marginBottom: 16,
            }}
          >
            <GlobalOutlined style={{ fontSize: 32, color: '#1677ff' }} />
          </div>
          <Title level={3} style={{ margin: 0, fontWeight: 700, color: '#111827' }}>
            BIDADYU BT BULUT
          </Title>
          <Text type="secondary" style={{ fontSize: 13 }}>
            Üniversite Merkezi BT Yönetim & Dağıtım Platformu
          </Text>
        </div>

        <Form name="login" onFinish={onFinish} layout="vertical" initialValues={{ username: 'admin', password: 'Admin123!' }}>
          <Form.Item
            name="username"
            label="Kullanıcı Adı"
            rules={[{ required: true, message: 'Kullanıcı adı giriniz!' }]}
          >
            <Input size="large" prefix={<UserOutlined style={{ color: '#9ca3af' }} />} placeholder="Örn: admin" />
          </Form.Item>

          <Form.Item
            name="password"
            label="Şifre"
            rules={[{ required: true, message: 'Şifre giriniz!' }]}
          >
            <Input.Password size="large" prefix={<LockOutlined style={{ color: '#9ca3af' }} />} placeholder="Şifre" />
          </Form.Item>

          <Form.Item style={{ marginTop: 24 }}>
            <Button type="primary" htmlType="submit" size="large" loading={loading} block style={{ height: 48, fontWeight: 600, fontSize: 16, borderRadius: 8 }}>
              Sisteme Giriş Yap
            </Button>
          </Form.Item>
        </Form>

        <div style={{ textAlign: 'center', marginTop: 16, color: '#9ca3af', fontSize: 12 }}>
          <Space>
            <SafetyOutlined /> Güvenli 256-Bit SSL Şifreli Bağlantı
          </Space>
        </div>
      </Card>
    </div>
  );
};
