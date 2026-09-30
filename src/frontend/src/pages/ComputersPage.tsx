import React, { useEffect, useState } from 'react';
import {
  Table,
  Tag,
  Button,
  Card,
  Input,
  Popconfirm,
  Tooltip,
  Badge,
  Space,
  Modal,
  Typography,
  Row,
  Col,
  Statistic,
  message,
} from 'antd';
import {
  SearchOutlined,
  DeleteOutlined,
  ReloadOutlined,
  CheckCircleOutlined,
  DesktopOutlined,
  LaptopOutlined,
  AppstoreOutlined,
  SafetyCertificateOutlined,
  CheckCircleTwoTone,
  ClockCircleTwoTone,
} from '@ant-design/icons';
import api, { getComputers, approveAgent } from '../services/api';

const { Text } = Typography;

const STATUS_COLOR: Record<string, string> = {
  Online: 'green',
  Pending: 'orange',
  Offline: 'red',
};
const STATUS_LABEL: Record<string, string> = {
  Online: '🟢 Çevrimiçi',
  Pending: '🟡 Onay Bekliyor',
  Offline: '🔴 Çevrimdışı',
};

export const ComputersPage: React.FC = () => {
  const [computers, setComputers] = useState<any[]>([]);
  const [selectedRowKeys, setSelectedRowKeys] = useState<React.Key[]>([]);
  const [searchText, setSearchText] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);

  // Envanter Modal State'leri
  const [isSoftwareModalOpen, setIsSoftwareModalOpen] = useState<boolean>(false);
  const [selectedComputer, setSelectedComputer] = useState<any>(null);
  const [installedSoftware, setInstalledSoftware] = useState<any[]>([]);
  const [softwareLoading, setSoftwareLoading] = useState<boolean>(false);
  const [softwareSearchText, setSoftwareSearchText] = useState<string>('');

  const fetchComputers = async (silent = false) => {
    if (!silent) setLoading(true);
    try {
      const res = await getComputers();
      setComputers(res.data);
    } catch {
      if (!silent) message.error('Bilgisayarlar yüklenemedi.');
    } finally {
      if (!silent) setLoading(false);
    }
  };

  useEffect(() => {
    fetchComputers();
    const interval = setInterval(() => fetchComputers(true), 5000);
    return () => clearInterval(interval);
  }, []);

  const handleApprove = async (id: string) => {
    try {
      await approveAgent(id);
      message.success('Agent onaylandı! Bilgisayar sisteme dahil edildi.');
      fetchComputers();
    } catch {
      message.error('Onaylama başarısız.');
    }
  };

  const handleDelete = async (id: string) => {
    try {
      await api.delete(`/Computers/${id}`);
      message.success('Bilgisayar silindi.');
      fetchComputers();
    } catch {
      message.error('Silme başarısız.');
    }
  };

  const handleBulkDelete = async () => {
    if (selectedRowKeys.length === 0) return;
    try {
      await api.post('/Computers/bulk-delete', selectedRowKeys);
      message.success(`${selectedRowKeys.length} bilgisayar silindi.`);
      setSelectedRowKeys([]);
      fetchComputers();
    } catch {
      message.error('Toplu silme başarısız.');
    }
  };

  const handleOpenSoftwareModal = async (computer: any) => {
    setSelectedComputer(computer);
    setIsSoftwareModalOpen(true);
    setSoftwareLoading(true);
    setSoftwareSearchText('');
    try {
      const res = await api.get(`/Computers/${computer.id}/software`);
      setInstalledSoftware(res.data);
    } catch {
      message.error('Kurulu yazılımlar yüklenemedi.');
    } finally {
      setSoftwareLoading(false);
    }
  };

  const isVirtual = (c: any) => c.ipAddress?.startsWith('192.168.10.');
  const isReal = (c: any) => !isVirtual(c);

  const filteredComputers = computers.filter(
    (c) =>
      c.hostname?.toLowerCase().includes(searchText.toLowerCase()) ||
      c.ipAddress?.toLowerCase().includes(searchText.toLowerCase()) ||
      c.id?.toLowerCase().includes(searchText.toLowerCase())
  );

  const filteredInstalledSoftware = installedSoftware.filter(
    (s) =>
      s.softwareName?.toLowerCase().includes(softwareSearchText.toLowerCase()) ||
      s.publisher?.toLowerCase().includes(softwareSearchText.toLowerCase())
  );

  const realCount = computers.filter(isReal).length;
  const virtualCount = computers.filter(isVirtual).length;
  const onlineCount = computers.filter((c) => c.status === 'Online').length;
  const pendingCount = computers.filter((c) => c.status === 'Pending').length;

  const columns = [
    {
      title: 'Cihaz ID',
      dataIndex: 'id',
      key: 'id',
      width: 130,
      render: (id: string) => (
        <Tooltip title={`Full Computer ID: ${id}`}>
          <Tag color="geekblue" style={{ fontFamily: 'monospace', fontWeight: 600 }}>
            #{id ? id.substring(0, 8).toUpperCase() : 'N/A'}
          </Tag>
        </Tooltip>
      ),
    },
    {
      title: 'Bilgisayar Adı & Türü',
      dataIndex: 'hostname',
      key: 'hostname',
      sorter: (a: any, b: any) => a.hostname.localeCompare(b.hostname),
      render: (hostname: string, record: any) => (
        <Space direction="vertical" size={2}>
          <Space>
            {isVirtual(record) ? (
              <LaptopOutlined style={{ color: '#722ed1', fontSize: 16 }} />
            ) : (
              <DesktopOutlined style={{ color: '#1677ff', fontSize: 16 }} />
            )}
            <Text strong style={{ fontSize: 14 }}>{hostname}</Text>
          </Space>
          <Space size={4}>
            {isVirtual(record) && <Tag color="purple">SANAL</Tag>}
            {!isVirtual(record) && record.status !== 'Pending' && (
              <Tag color="blue">GERÇEK AGENT</Tag>
            )}
            {record.status === 'Pending' && <Tag color="orange">ONAY BEKLİYOR</Tag>}
          </Space>
        </Space>
      ),
    },
    {
      title: 'IP Adresi',
      dataIndex: 'ipAddress',
      key: 'ipAddress',
      render: (ip: string) =>
        ip ? <code style={{ fontSize: 13 }}>{ip}</code> : <span style={{ color: '#ccc' }}>—</span>,
    },
    {
      title: 'İşletim Sistemi',
      dataIndex: 'osVersion',
      key: 'osVersion',
      render: (v: string) => v || <span style={{ color: '#ccc' }}>—</span>,
    },
    {
      title: 'Son Aktiflik (Last Seen)',
      dataIndex: 'lastSeen',
      key: 'lastSeen',
      render: (v: string) => {
        if (!v) return <span style={{ color: '#ccc' }}>—</span>;
        const dateStr = v.endsWith('Z') || v.includes('+') ? v : v + 'Z';
        const diffSeconds = Math.round((new Date().getTime() - new Date(dateStr).getTime()) / 1000);
        if (diffSeconds >= 0 && diffSeconds < 35) {
          return <Tag color="green">🟢 Canlı ({diffSeconds} sn önce)</Tag>;
        }
        return (
          <Tooltip title={new Date(dateStr).toLocaleString('tr-TR')}>
            <Tag color="red">
              🔴{' '}
              {diffSeconds > 3600
                ? `${Math.round(diffSeconds / 3600)} sa`
                : `${Math.round(diffSeconds / 60)} dk`}{' '}
              önce
            </Tag>
          </Tooltip>
        );
      },
    },
    {
      title: 'Durum',
      dataIndex: 'status',
      key: 'status',
      filters: [
        { text: 'Çevrimiçi', value: 'Online' },
        { text: 'Onay Bekliyor', value: 'Pending' },
        { text: 'Çevrimdışı', value: 'Offline' },
      ],
      onFilter: (value: any, record: any) => record.status === value,
      render: (status: string) => (
        <Tag color={STATUS_COLOR[status] ?? 'default'} style={{ fontSize: 12, padding: '4px 8px' }}>
          {STATUS_LABEL[status] ?? status}
        </Tag>
      ),
    },
    {
      title: 'İşlemler / Envanter',
      key: 'action',
      render: (_: any, record: any) => (
        <Space>
          <Button
            size="small"
            icon={<AppstoreOutlined />}
            onClick={() => handleOpenSoftwareModal(record)}
          >
            Yazılım Envanteri
          </Button>
          {record.status === 'Pending' && (
            <Tooltip title="Bu bilgisayarı sisteme kabul et">
              <Button
                type="primary"
                size="small"
                icon={<CheckCircleOutlined />}
                onClick={() => handleApprove(record.id)}
              >
                Onayla
              </Button>
            </Tooltip>
          )}
          <Popconfirm
            title="Bu bilgisayarı silmek istediğinize emin misiniz?"
            onConfirm={() => handleDelete(record.id)}
            okText="Evet, Sil"
            cancelText="İptal"
          >
            <Button danger size="small" icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  const softwareColumns = [
    {
      title: 'Yazılım Adı',
      dataIndex: 'softwareName',
      key: 'softwareName',
      render: (text: string) => (
        <Space>
          <SafetyCertificateOutlined style={{ color: '#52c41a' }} />
          <Text strong>{text}</Text>
        </Space>
      ),
    },
    {
      title: 'Yayıncı / Yapımcı',
      dataIndex: 'publisher',
      key: 'publisher',
      render: (v: string) => v || 'Bilinmiyor',
    },
    {
      title: 'Kurulu Sürüm',
      dataIndex: 'version',
      key: 'version',
      render: (v: string) => <Tag color="green">v{v || '1.0'}</Tag>,
    },
    {
      title: 'Tespit Edilme Tarihi',
      dataIndex: 'lastSeenAt',
      key: 'lastSeenAt',
      render: (v: string) => (v ? new Date(v).toLocaleString('tr-TR') : '—'),
    },
  ];

  return (
    <Space direction="vertical" size="large" style={{ width: '100%' }}>
      {/* Üst İstatistik Kartları */}
      <Row gutter={16}>
        <Col span={6}>
          <Card className="stat-card" bordered={false}>
            <Statistic
              title="Toplam Bilgisayarlar"
              value={computers.length}
              prefix={<DesktopOutlined style={{ color: '#1677ff' }} />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card className="stat-card" bordered={false}>
            <Statistic
              title="Aktif Çevrimiçi (Online)"
              value={onlineCount}
              valueStyle={{ color: '#3f8600' }}
              prefix={<CheckCircleTwoTone twoToneColor="#52c41a" />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card className="stat-card" bordered={false}>
            <Statistic
              title="Onay Bekleyenler"
              value={pendingCount}
              valueStyle={{ color: '#cf1322' }}
              prefix={<ClockCircleTwoTone twoToneColor="#faad14" />}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card className="stat-card" bordered={false}>
            <Statistic
              title="Gerçek LAN Agentlar"
              value={realCount}
              prefix={<Badge status="processing" text={`${virtualCount} Sanal`} />}
            />
          </Card>
        </Col>
      </Row>

      {/* Ana Tablo Kartı */}
      <Card
        bordered={false}
        style={{ borderRadius: 12, boxShadow: '0 1px 3px rgba(0,0,0,0.05)' }}
        title={
          <Space size="large">
            <span style={{ fontWeight: 600, fontSize: 16 }}>Bilgisayar & Envanter Listesi</span>
          </Space>
        }
        extra={
          <Space size="middle">
            {selectedRowKeys.length > 0 && (
              <Popconfirm
                title={`${selectedRowKeys.length} bilgisayarı silmek istiyor musunuz?`}
                onConfirm={handleBulkDelete}
                okText="Evet, Sil"
                cancelText="İptal"
              >
                <Button danger icon={<DeleteOutlined />}>
                  Seçili Sil ({selectedRowKeys.length})
                </Button>
              </Popconfirm>
            )}
            <Input
              placeholder="Cihaz ID, Ad veya IP ara..."
              prefix={<SearchOutlined style={{ color: '#bfbfbf' }} />}
              value={searchText}
              onChange={(e) => setSearchText(e.target.value)}
              style={{ width: 260 }}
              allowClear
            />
            <Tooltip title="Listeyi Yenile">
              <Button icon={<ReloadOutlined />} onClick={() => fetchComputers()} />
            </Tooltip>
          </Space>
        }
      >
        {pendingCount > 0 && (
          <div
            style={{
              background: '#fffbe6',
              border: '1px solid #ffe58f',
              borderRadius: 8,
              padding: '12px 16px',
              marginBottom: 20,
              color: '#856404',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'space-between',
            }}
          >
            <span>
              ⚠️ <strong>{pendingCount} adet bilgisayar onay bekliyor.</strong> Sisteme kabul etmek için satırdaki <strong>Onayla</strong> butonuna tıklayın.
            </span>
          </div>
        )}

        <Table
          rowSelection={{ selectedRowKeys, onChange: setSelectedRowKeys }}
          dataSource={filteredComputers}
          columns={columns}
          rowKey="id"
          loading={loading}
          pagination={{ pageSize: 8, showSizeChanger: true }}
          locale={{ emptyText: 'Hiç bilgisayar bulunamadı. LAN Agent kurarak bilgisayar ekleyebilirsiniz.' }}
        />
      </Card>

      {/* ENVANTER MODALI */}
      <Modal
        title={
          <Space>
            <DesktopOutlined style={{ color: '#1677ff' }} />
            <span>{selectedComputer?.hostname} — Kurulu Yazılımlar Envanteri (Registry Audit)</span>
          </Space>
        }
        open={isSoftwareModalOpen}
        onCancel={() => setIsSoftwareModalOpen(false)}
        footer={[
          <Button key="close" type="primary" onClick={() => setIsSoftwareModalOpen(false)}>
            Kapat
          </Button>,
        ]}
        width={760}
      >
        <div style={{ marginBottom: 16, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Text type="secondary">
            Windows Kayıt Defteri (Registry) taranarak otomatik olarak tespit edilen programlar:
          </Text>
          <Input
            placeholder="Yazılım adı ara..."
            prefix={<SearchOutlined />}
            value={softwareSearchText}
            onChange={(e) => setSoftwareSearchText(e.target.value)}
            style={{ width: 240 }}
            allowClear
          />
        </div>
        <Table
          dataSource={filteredInstalledSoftware}
          columns={softwareColumns}
          rowKey="id"
          loading={softwareLoading}
          pagination={{ pageSize: 7 }}
          locale={{ emptyText: 'Bu bilgisayarda taranmış kurulu yazılım bulunamadı.' }}
        />
      </Modal>
    </Space>
  );
};
