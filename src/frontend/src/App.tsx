import React, { useState } from 'react';
import { LoginPage } from './pages/LoginPage';
import { ComputersPage } from './pages/ComputersPage';
import { SoftwarePage } from './pages/SoftwarePage';
import { DeploymentsPage } from './pages/DeploymentsPage';
import { CloudStoragePage } from './pages/CloudStoragePage';
import { VirtualLabPage } from './pages/VirtualLabPage';
import { Layout, Menu, Typography, Button, Space, Avatar, Badge, Dropdown, ConfigProvider, Tooltip, Drawer, Empty, Tag, List, message } from 'antd';
import {
  DesktopOutlined,
  AppstoreOutlined,
  SendOutlined,
  CloudUploadOutlined,
  ClusterOutlined,
  LogoutOutlined,
  UserOutlined,
  BellOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  SyncOutlined,
  DeleteOutlined,
} from '@ant-design/icons';
import bidbLogo from './assets/bidb.png';
import api from './services/api';

const { Header, Content, Sider } = Layout;

export const App: React.FC = () => {
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [activeTab, setActiveTab] = useState<string>('computers');
  const [collapsed, setCollapsed] = useState<boolean>(false);
  const [notifications, setNotifications] = useState<any[]>([]);
  const [unreadCount, setUnreadCount] = useState<number>(0);
  const [isDrawerOpen, setIsDrawerOpen] = useState<boolean>(false);

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    setToken(null);
  };

  const fetchNotifications = async () => {
    try {
      const res = await api.get('/Notifications');
      setNotifications(res.data.notifications || []);
      setUnreadCount(res.data.unreadCount || 0);
    } catch { }
  };

  React.useEffect(() => {
    if (token) {
      fetchNotifications();
      const interval = setInterval(fetchNotifications, 5000);
      return () => clearInterval(interval);
    }
  }, [token]);



  const handleSetStatus = async (item: any, status: string) => {
    try {
      await api.put(`/Notifications/${item.id}/status`, { status });
      message.success(`Durum güncellendi: ${status === 'InProgress' ? 'Şu An İlgileniliyor' : 'Tamamlandı'}`);
      fetchNotifications();
    } catch {
      message.error('Durum güncellenemedi.');
    }
  };

  const handleDeleteNotification = async (id: string) => {
    try {
      await api.delete(`/Notifications/${id}`);
      fetchNotifications();
    } catch { }
  };

  if (!token) {
    return <LoginPage onLoginSuccess={() => setToken(localStorage.getItem('token'))} />;
  }

  const userJson = localStorage.getItem('user');
  const user = userJson ? JSON.parse(userJson) : { username: 'Admin User' };

  return (
    <ConfigProvider
      theme={{
        token: {
          colorPrimary: '#1677ff',
          borderRadius: 8,
          fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
        },
      }}
    >
      <Layout style={{ minHeight: '100vh', background: '#f4f6f9' }}>
        <Sider
          collapsible
          collapsed={collapsed}
          onCollapse={(value) => setCollapsed(value)}
          width={260}
          style={{
            background: '#001529',
            boxShadow: '2px 0 8px rgba(0, 21, 41, 0.15)',
            zIndex: 11,
          }}
        >
          <div className={`brand-logo-box ${collapsed ? 'collapsed' : ''}`}>
            <img
              src={bidbLogo}
              alt="BİDB Logo"
              style={{
                width: collapsed ? 36 : 38,
                height: collapsed ? 36 : 38,
                objectFit: 'contain',
                borderRadius: '50%',
                background: '#ffffff',
                padding: 2,
                boxShadow: '0 0 8px rgba(22, 119, 255, 0.4)',
                transition: 'all 0.2s ease',
              }}
            />
            {!collapsed && (
              <div>
                <div className="brand-title">BIDADYU BT BULUT</div>
                <div style={{ fontSize: 10, color: 'rgba(255,255,255,0.45)' }}>Yönetim Merkezi v2.0</div>
              </div>
            )}
          </div>
          <Menu
            theme="dark"
            selectedKeys={[activeTab]}
            onClick={(e) => setActiveTab(e.key)}
            mode="inline"
            style={{ padding: '8px 0' }}
            items={[
              {
                key: 'computers',
                icon: <DesktopOutlined style={{ fontSize: 16 }} />,
                label: 'Cihazlar & Agentlar',
              },
              {
                key: 'software',
                icon: <AppstoreOutlined style={{ fontSize: 16 }} />,
                label: 'Yazılım Kataloğu',
              },
              {
                key: 'storage',
                icon: <CloudUploadOutlined style={{ fontSize: 16 }} />,
                label: 'Bulut Depo & Paketler',
              },
              {
                key: 'virtuallab',
                icon: <ClusterOutlined style={{ fontSize: 16 }} />,
                label: 'Sanal Lab / PC Üretici',
              },
              {
                key: 'deployments',
                icon: <SendOutlined style={{ fontSize: 16 }} />,
                label: 'Dağıtım Görevleri',
              },
            ]}
          />
        </Sider>
        <Layout>
          <Header
            className="app-header"
            style={{
              padding: '0 24px',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              height: 64,
            }}
          >
            <Typography.Title level={4} style={{ margin: 0, fontWeight: 600, color: '#1f2937' }}>
              {activeTab === 'computers' && '🖥️ Cihaz & Agent Yönetim Merkezi'}
              {activeTab === 'software' && '📦 Merkezi Yazılım Kataloğu'}
              {activeTab === 'storage' && '☁️ Bulut Depo & Paket Yönetimi'}
              {activeTab === 'virtuallab' && '🔬 Sanal Lab & Otomatik PC Üretici'}
              {activeTab === 'deployments' && '🚀 Canlı Yazılım Dağıtım Görevleri (Jobs)'}
            </Typography.Title>

            <Space size="large">
              <Tooltip title="Gelen Arıza & Eksik Program Bildirimleri">
                <Badge count={unreadCount} overflowCount={99}>
                  <Button
                    type="text"
                    icon={<BellOutlined style={{ fontSize: 20, color: unreadCount > 0 ? '#ff4d4f' : '#4b5563' }} />}
                    onClick={() => {
                      fetchNotifications();
                      setIsDrawerOpen(true);
                    }}
                  />
                </Badge>
              </Tooltip>

              <Dropdown
                menu={{
                  items: [
                    {
                      key: 'logout',
                      icon: <LogoutOutlined />,
                      label: 'Güvenli Çıkış Yap',
                      danger: true,
                      onClick: handleLogout,
                    },
                  ],
                }}
                placement="bottomRight"
                trigger={['click']}
              >
                <div
                  style={{
                    cursor: 'pointer',
                    padding: '6px 12px',
                    borderRadius: 8,
                    background: '#ffffff',
                    border: '1px solid #e5e7eb',
                    display: 'flex',
                    alignItems: 'center',
                    gap: 10,
                    transition: 'all 0.2s ease',
                    boxShadow: '0 1px 2px rgba(0, 0, 0, 0.05)',
                  }}
                >
                  <Avatar style={{ backgroundColor: '#1677ff', flexShrink: 0 }} icon={<UserOutlined />} />
                  <div style={{ lineHeight: 1.2, textAlign: 'left' }}>
                    <div style={{ fontWeight: 600, fontSize: 13, color: '#1f2937' }}>{user.username || 'Admin User'}</div>
                    <div style={{ fontSize: 11, color: '#6b7280' }}>Sistem Yöneticisi</div>
                  </div>
                </div>
              </Dropdown>
            </Space>
          </Header>
          <Content style={{ margin: '24px', minHeight: 280 }}>
            {activeTab === 'computers' && <ComputersPage />}
            {activeTab === 'software' && <SoftwarePage />}
            {activeTab === 'storage' && <CloudStoragePage />}
            {activeTab === 'virtuallab' && <VirtualLabPage />}
            {activeTab === 'deployments' && <DeploymentsPage />}
          </Content>
        </Layout>
      </Layout>

      <Drawer
        title="🔔 Gelen Cihaz & Agent Bildirimleri"
        placement="right"
        width={440}
        open={isDrawerOpen}
        onClose={() => setIsDrawerOpen(false)}
      >
        {notifications.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '60px 20px' }}>
            <Empty description="Şu anlık bildirim yok" image={Empty.PRESENTED_IMAGE_SIMPLE} />
          </div>
        ) : (
          <List
            itemLayout="vertical"
            dataSource={notifications}
            renderItem={(item) => {
              const isPending = item.status === 'Pending' || !item.isRead;
              const isInProgress = item.status === 'InProgress';

              return (
                <List.Item
                  key={item.id}
                  style={{
                    padding: 16,
                    marginBottom: 12,
                    borderRadius: 8,
                    background: isPending ? '#fffbe6' : isInProgress ? '#e6f7ff' : '#fafafa',
                    border: isPending ? '1px solid #ffe58f' : isInProgress ? '1px solid #91caff' : '1px solid #f0f0f0',
                  }}
                  actions={[
                    item.status !== 'Completed' && (
                      <Button
                        type="primary"
                        size="small"
                        icon={<SyncOutlined spin={isInProgress} />}
                        style={{ backgroundColor: isInProgress ? '#52c41a' : '#1677ff' }}
                        onClick={() => handleSetStatus(item, isInProgress ? 'Completed' : 'InProgress')}
                      >
                        {isInProgress ? '✅ Tamamlandı Yap' : '🛠️ Şu An İlgileniliyor'}
                      </Button>
                    ),
                    <Button
                      type="text"
                      danger
                      size="small"
                      icon={<DeleteOutlined />}
                      onClick={() => handleDeleteNotification(item.id)}
                    />
                  ]}
                >
                  <List.Item.Meta
                    title={
                      <Space style={{ width: '100%', justifyContent: 'space-between' }}>
                        <Typography.Text strong style={{ fontSize: 14 }}>
                          {item.title}
                        </Typography.Text>
                        {item.status === 'InProgress' ? (
                          <Tag color="processing" icon={<SyncOutlined spin />}>
                            Şu An İlgileniliyor
                          </Tag>
                        ) : item.status === 'Completed' ? (
                          <Tag color="success" icon={<CheckCircleOutlined />}>
                            Tamamlandı
                          </Tag>
                        ) : (
                          <Tag color="warning" icon={<ClockCircleOutlined />}>
                            Bekliyor
                          </Tag>
                        )}
                      </Space>
                    }
                    description={
                      <div style={{ marginTop: 4 }}>
                        <div style={{ color: '#374151', fontSize: 13, marginBottom: 6 }}>{item.message}</div>
                        <div style={{ color: '#9ca3af', fontSize: 11 }}>
                          📅 {new Date(item.createdAt).toLocaleString('tr-TR')}
                        </div>
                      </div>
                    }
                  />
                </List.Item>
              );
            }}
          />
        )}
      </Drawer>
    </ConfigProvider>
  );
};

export default App;
